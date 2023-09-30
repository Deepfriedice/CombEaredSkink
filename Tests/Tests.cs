using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChessChallenge;
using ChessChallenge.API;
using ChessChallenge.Application;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Linq;

// Testing idea copied from https://github.com/KennethOnGitHub/KnightToE4F
namespace MyBotTests
{
    [TestClass]
    public class EvaluateTests
    {

        // test the score returned by searching a number of positions
        [TestMethod]
        [DataRow(6, 0, "8/8/8/8/8/8/8/8 w - - 0 1")]
        [DataRow(6, 0, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
        [DataRow(6, -8, "r1b1k2r/ppp3pp/2n5/1Bq1pp2/8/P1P2N2/1P3PPP/R1BQ1RK1 b kq - 0 11")]
        [DataRow(6, -46, "8/8/4q3/1k6/6N1/2K3r1/8/8 w - - 0 1")]
        [DataRow(6, -29, "8/8/4q3/1k6/6N1/2K5/8/8 w - - 0 1")]
        [DataRow(6, 29, "8/8/4q3/1k6/6N1/2K5/8/8 b - - 0 1")]
        [DataRow(3, 17, "r2r2k1/p1q1pp1p/1p4p1/3P4/2P5/6P1/P4P1P/1R1QR1K1 w - - 2 20")]
        [DataRow(3, 6, "r1bq1k1r/1p3ppp/p3pn2/2b1N3/3p1B2/2P3Q1/PP2BPPP/3R1RK1 b - - 0 15")]
        [DataRow(3, 23, "2rqrbk1/3n1ppp/pp3n2/3pN3/P1pP1B2/2P5/1PQN1PPP/R3R1K1 w - - 2 17")]
        public void Search_OnPositions(int depth, int expected, string position)
        {
            var bot = new MyBot();
            Board board = Board.CreateBoardFromFEN(position);
            int score = bot.Search(board, depth, int.MinValue, int.MaxValue);
            Console.WriteLine(board.CreateDiagram());  // print failing board and scores
            Console.WriteLine("Expected score: {0}", expected);
            Console.WriteLine("Calculated score: {0}", score);
            Assert.AreEqual(expected, score);
        }

        // test the number of evaluations required by search
        [TestMethod]
        [DataRow(6, 845677, "r1b1k2r/ppp3pp/2n5/1Bq1pp2/8/P1P2N2/1P3PPP/R1BQ1RK1 b kq - 0 11")]
        [DataRow(6, 2010528, "r4rk1/pp3ppb/2p4p/b3q3/4P3/2N2QPB/P6P/4RR1K w - - 2 25")]
        public void Search_CountEvaluations(int depth, int expected, string position)
        {
            var bot = new MyBot();
            Board board = Board.CreateBoardFromFEN(position);
            bot.Search(board, depth, int.MinValue, int.MaxValue);
            Console.WriteLine("FEN: {0}", position);
            Console.WriteLine("Expected eval count: {0}", expected);
            Console.WriteLine("Actual eval count: {0}", bot.evalCount);
            Assert.AreEqual(expected, bot.evalCount);
        }

        // the rules require the transposition table to be less than 256MB
        [TestMethod]
        public void EvalCacheSize()
        {
            var bot = new MyBot();
            int itemSize = Marshal.SizeOf<MyBot.CacheItem>();
            int tableLength = bot.evalCache.Length;
            int tableSize = itemSize * tableLength;
            Console.WriteLine("table size: {0}MB", tableSize / (1<<20));
            Assert.IsTrue(tableSize <= (1<<28));
        }

        [TestMethod]
        public void FindBestMoves_Blunder()
        {
            var bot = new MyBot();
            string position = "r1bqkb1r/pppppppp/2n2n2/8/8/N3P3/PPPP1PPP/R1BQKBNR w KQkq - 3 3";
            Board board = Board.CreateBoardFromFEN(position);
            Console.WriteLine(board.CreateDiagram());
            List<Move> moves = bot.FindBestMoves(board, 5);
            string[] moveNames = moves.Select(m => MoveDisplayer.MoveName(m)).ToArray();
            Array.Sort(moveNames);
            Console.WriteLine("Best moves: {0}", moveNames);
            Assert.IsFalse(moveNames.Contains("d1h5"));  // d1h5 is a massive blunder
            Assert.IsFalse(moveNames.Contains("d1g4"));  // d1g4 is too
        }

    }
}