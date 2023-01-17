using Microsoft.VisualStudio.TestTools.UnitTesting;
using EloEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EloEngine.Tests
{
    [TestClass()]
    public class EloTests
    {
        private readonly Elo elo;

        public EloTests() { this.elo = new Elo(); }

        [TestMethod()]
        [DataRow(32, 1500, 1500, .5)]
        [DataRow(32, 1700, 1300, 0.9090909090909091)]
        public void ExpectationToWinTest(int eloK, int rating1, int rating2, double expected)
        {

            Elo.EloK = eloK;
            var probablity = elo.ExpectationToWin(rating1, rating2);
            Assert.AreEqual(expected, probablity);
        }

        [TestMethod()]
        [DataRow(32, 1700, 1300, GameOutcome.Player1Win, 1702, 1298)]
        [DataRow(32, 1700, 1300, GameOutcome.Player1Loss, 1671, 1329)]
        [DataRow(32, 1700, 1300, GameOutcome.Player1Tie, 1687, 1313)]
        public void CalculateEloTest(int eloK, int rating1, int rating2, GameOutcome gameOutcome, int expectedRating1, int expectedRating2)
        {
            Elo.EloK = eloK;
            var result = elo.CalculateELO(rating1, rating2, gameOutcome);
            Assert.AreEqual(expectedRating1, result.Player1Rating );
            Assert.AreEqual(expectedRating2, result.Player2Rating);
        }
    }
}