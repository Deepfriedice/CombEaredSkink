using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private readonly Random rng = new();
    private const int UpperBound = 1000;
    private int searchDepth = 2;

    // Try to evaluate how good a position is.
    // Higher number -> better for the current player.
    private int BoardScore(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        int ownMoveCount = moves.Length;
        board.ForceSkipTurn();
        int otherMoveCount = board.GetLegalMoves().Length;
        board.UndoSkipTurn();
        return ownMoveCount - otherMoveCount;
        // note that board.IsInCheck is forced to false
    }

    private int RecursiveBoardScore(Board board, int depth)
    {
        // avoid checkmate and draws
        if (board.IsInCheckmate())
        {
            return board.PlyCount - UpperBound;
        }
        if (board.IsDraw())
        {
            return 0;
        }

        if (depth == 0)
        {
            return BoardScore(board);
        }

        int score = -UpperBound;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return UpperBound - board.PlyCount - 1;
            }

            // update the list of best moves
            int opponentScore = RecursiveBoardScore(board, depth-1);
            score = Math.Max(score, -opponentScore);
            board.UndoMove(move);
        }

        // play a random move from the best moves
        return score;
    }

    // Select a random move from a list.
    private Move RandomMove(IList<Move> moves)
    {
        return moves[rng.Next(moves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        if (board.PlyCount < 2)  // at game start
        {
            searchDepth = 2;
        }
        bool wasInCheck = board.IsInCheck();
        Console.WriteLine("search depth {0}", searchDepth);  //#DEBUG

        int bestScore = -UpperBound;
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

            // update the list of best moves
            int moveScore = -RecursiveBoardScore(board, searchDepth);
            displayer.Add(move, moveScore);  //#DEBUG
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
        if (!wasInCheck)  // don't update the search depth in special cases
        {
            if (timer.MillisecondsElapsedThisTurn > 2000 & searchDepth > 0)
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