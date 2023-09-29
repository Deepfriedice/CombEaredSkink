using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private readonly Random rng = new();
    private const int MaxScore = 1000;
    private const int TimeConsumption = 12;  // faction of the clock to use each turn
    private const int TimeThreshold = 16;  // how aggressively to increase the search depth
    private const double InitialSearchScale = 1.8;  // coefficient for initial searchDepth value
    private int searchDepth = 0;
    internal int evalCount = 0;  //#DEBUG
    internal int cacheHits = 0;  //#DEBUG
    internal int cacheMisses = 0;  //#DEBUG
    internal int cacheEvictions = 0;  //#DEBUG

    private class CacheItem
    {
        public CacheItem? prev;
        public CacheItem? next;
        public ulong key;
        public int score;
    }

    private Dictionary<ulong, CacheItem> evalCache = new Dictionary<ulong, CacheItem>();
    private CacheItem? evalCacheHead;  // head is first
    private CacheItem? evalCacheTail;
    private const int EvalCacheMaxLength = 1<<24;



    // How long do we have to think about our move?
    private int ThinkingTimeGoal(Timer timer)
    {
        int remaining = timer.MillisecondsRemaining / TimeConsumption;
        int evenSplit = timer.GameStartTimeMilliseconds / (2 * TimeConsumption);
        int goal = Math.Min(remaining, evenSplit) + timer.IncrementMilliseconds;
        return Math.Min(goal, timer.MillisecondsRemaining / 2) + 1;
    }

    // Try to evaluate how good a position is.
    // Higher number -> better for the current player.
    private int BoardScore(Board board)
    {
        evalCount++;  //#DEBUG
        Move[] moves = board.GetLegalMoves();
        int ownMoveCount = moves.Length;
        board.ForceSkipTurn();
        int otherMoveCount = board.GetLegalMoves().Length;
        board.UndoSkipTurn();
        return ownMoveCount - otherMoveCount;
        // note that board.IsInCheck is forced to false
    }

    private int CachedBoardScore(Board board)
    {
        ulong key = board.ZobristKey;
        CacheItem? cacheItem;

        if (evalCache.TryGetValue(key, out cacheItem))
        {
            cacheHits++;  //#DEBUG
            // linked list bullshit - move cacheItem to the front of the list
            if (evalCacheHead != cacheItem)
            {
                if (evalCacheTail == cacheItem)
                    evalCacheTail = cacheItem.prev;
                else
                    cacheItem.next.prev = cacheItem.prev;
                cacheItem.prev.next = cacheItem.next;
                cacheItem.prev = null;
                cacheItem.next = evalCacheHead;
                if (evalCacheHead != null)
                    evalCacheHead.prev = cacheItem;
                evalCacheHead = cacheItem;
            }
        }
        else
        {
            cacheMisses++;  //#DEBUG
            cacheItem = new CacheItem();
            cacheItem.score = BoardScore(board);
            cacheItem.key = key;
            evalCache[key] = cacheItem;
            cacheItem.prev = null;
            cacheItem.next = evalCacheHead;
            if (evalCacheHead != null)
                evalCacheHead.prev = cacheItem;
            evalCacheHead = cacheItem;
            if (evalCacheTail == null)
            {
                evalCacheTail = cacheItem;
            }
        }

        if (evalCache.Count > EvalCacheMaxLength)
        {
            evalCache.Remove(evalCacheTail.key);
            evalCacheTail = evalCacheTail.prev;
            evalCacheTail.next = null;
            cacheEvictions++;
        }

        return cacheItem.score;
    }

    // Produce an array of moves sorted by estimate, probably-better moves first.
    private Move[] SortedMoves(Board board, int depth)
    {
        Move[] moves = board.GetLegalMoves();
        if (depth >= 2)
        {
            int[] moveScores = new int[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                Move move = moves[i];
                board.MakeMove(move);
                moveScores[i] = CachedBoardScore(board);
                board.UndoMove(move);
            }
            Array.Sort(moveScores, moves);
        }
        return moves;
    }

    internal int RecursiveBoardScore(Board board, int depth, int lowerBound, int upperBound)
    {
        // avoid checkmate and draws
        if (board.IsInCheckmate())
            return board.PlyCount - MaxScore;

        if (board.IsDraw())
            return 0;

        if (depth == 0)
            return CachedBoardScore(board);

        // int bestScore = lowerBound;
        int bestScore = -MaxScore;
        foreach (Move move in SortedMoves(board, depth))
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return MaxScore - board.PlyCount - 1;
            }

            // search recursively
            // the quality of the move to us is how bad it makes the opponent's position
            int moveScore = -RecursiveBoardScore(board, depth-1, -upperBound,  -Math.Max(bestScore, lowerBound));
            board.UndoMove(move);

            // beta cut off
            if (moveScore > upperBound)
            {
                // stop evaluating this position, as it's
                // "too good" for the opponent to allow us to reach
                return MaxScore;
            }

            bestScore = Math.Max(bestScore, moveScore);
        }

        return bestScore;
    }

    // Select a random move from a list.
    private Move RandomMove(IList<Move> moves)
    {
        return moves[rng.Next(moves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        int thinkingTimeGoal = ThinkingTimeGoal(timer);
        evalCount = 0;  //#DEBUG
        cacheHits = 0;  //#DEBUG
        cacheMisses = 0;  //#DEBUG
        cacheEvictions = 0;  //#DEBUG

        // set initial search depth based on the game length
        if (searchDepth == 0)
            searchDepth = (int) (InitialSearchScale * Math.Log(timer.GameStartTimeMilliseconds, TimeThreshold));

        bool wasInCheck = board.IsInCheck();
        Console.WriteLine("search depth {0}", searchDepth);  //#DEBUG

        int bestScore = -MaxScore;
        MoveDisplayer displayer = new MoveDisplayer();  //#DEBUG
        List<Move> bestMoves = new List<Move>();
        foreach (Move move in SortedMoves(board, searchDepth))
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                Console.WriteLine("found checkmate: {0}", MoveDisplayer.MoveName(move));  //#DEBUG
                return move;  // don't need to undo the move
            }

            // evaluate the position after this move
            int moveScore = -RecursiveBoardScore(board, searchDepth-1, -MaxScore, -bestScore);

            if (moveScore != -MaxScore)  //#DEBUG
                displayer.Add(move, moveScore);  //#DEBUG

            // update the list of best moves
            if (moveScore == bestScore)
            {
                bestMoves.Add(move);
            }
            else if (moveScore > bestScore)
            {
                bestScore = moveScore;
                bestMoves.Clear();
                bestMoves.Add(move);
            }

            board.UndoMove(move);
        }

        displayer.Print();  //#DEBUG
        Console.WriteLine("evaluated positions: {0:D}k", evalCount/1000);  //#DEBUG
        Console.WriteLine("cache hits/misses/evictions: {0:D}/{1:D}/{2:D}", cacheHits, cacheMisses, cacheEvictions);  //#DEBUG
        Console.WriteLine("cache size: {0:D}", evalCache.Count);  //#DEBUG
        Console.WriteLine("thinking time: {0:D}/{1:D}ms", timer.MillisecondsElapsedThisTurn, thinkingTimeGoal);  //#DEBUG
        if (!wasInCheck)  // don't update the search depth in special cases
        {
            if (timer.MillisecondsElapsedThisTurn > thinkingTimeGoal && searchDepth > 1)
            {
                searchDepth--;
                Console.WriteLine("decreasing search depth");  //#DEBUG
            }
            else if (timer.MillisecondsElapsedThisTurn < thinkingTimeGoal / TimeThreshold)
            {
                searchDepth++;
                Console.WriteLine("increasing search depth");  //#DEBUG
            }
        }
        Console.WriteLine();  //#DEBUG

        // play a random move from the best moves
        return RandomMove(bestMoves);
    }
}