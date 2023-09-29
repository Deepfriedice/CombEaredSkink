using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private readonly Random rng = new();
    private const int MaxScore = 1000;
    private int searchDepth = 4;
    internal int evalCount = 0;  //#DEBUG

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

    internal int RecursiveBoardScore(Board board, int depth, int lowerBound, int upperBound)
    {
        // avoid checkmate and draws
        if (board.IsInCheckmate())
            return board.PlyCount - MaxScore;

        if (board.IsDraw())
            return 0;

        if (depth == 0)
            return BoardScore(board);

        // int bestScore = lowerBound;
        int bestScore = -MaxScore;
        foreach (Move move in board.GetLegalMoves())
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
        evalCount = 0;  //#DEBUG
        bool wasInCheck = board.IsInCheck();
        Console.WriteLine("search depth {0}", searchDepth);  //#DEBUG

        int bestScore = -MaxScore;
        MoveDisplayer displayer = new MoveDisplayer();  //#DEBUG
        List<Move> bestMoves = new List<Move>();
        foreach (Move move in board.GetLegalMoves())
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
        if (!wasInCheck)  // don't update the search depth in special cases
        {
            if (timer.MillisecondsElapsedThisTurn > 2000 & searchDepth > 1)
            {
                searchDepth--;
                Console.WriteLine("decreasing search depth");  //#DEBUG
            }
            else if (timer.MillisecondsElapsedThisTurn < 100)
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