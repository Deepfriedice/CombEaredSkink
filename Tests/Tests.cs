using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChessChallenge;
using ChessChallenge.API;
using ChessChallenge.Application;
using System.ComponentModel;
using System;

// Testing idea copied from https://github.com/KennethOnGitHub/KnightToE4F
namespace MyBotTests
{
    [TestClass]
    public class EvaluateTests
    {

        [TestMethod]
        [DataRow(0, "8/8/8/8/8/8/8/8 w - - 0 1")]
        [DataRow(0, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow(-19, "r1b1k2r/ppp3pp/2n5/1Bq1pp2/8/P1P2N2/1P3PPP/R1BQ1RK1 b kq - 0 11")]
        [DataRow(-43, "8/8/4q3/1k6/6N1/2K3r1/8/8 w - - 0 1")]
        [DataRow(-29, "8/8/4q3/1k6/6N1/2K5/8/8 w - - 0 1")]
        [DataRow(29, "8/8/4q3/1k6/6N1/2K5/8/8 b - - 0 1")]
        public void RecursiveBoardScore_OnPositions(int expected, string position)
        {
            var bot = new MyBot();
            Board board = Board.CreateBoardFromFEN(position);
            int score = bot.RecursiveBoardScore(board, 4, int.MinValue, int.MaxValue);
            Console.WriteLine(board.CreateDiagram());  // print failing board and scores
            Console.WriteLine("Expected score: {0}", expected);
            Console.WriteLine("Calculated score: {0}", score);
            Assert.AreEqual(score, expected);
        }

    }
}