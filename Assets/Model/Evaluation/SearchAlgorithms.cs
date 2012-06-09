﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reversi.Model.Evaluation;
using Reversi.Model.TranspositionTable;

namespace Reversi.Model.Evaluation
{
    public static class SearchAlgorithms
    {
        private const float Minimum = -1000000;
        const int InitialAlphaBeta = 10000;
        public static readonly float[] Sign = new[] { 1f, -1 };
        public static int MaxDepth = 7;

        public static float NegaMax(INode node, int colour, int depth, IList<INode> nodesSearched = null)
        {
            if (nodesSearched != null)
                nodesSearched.Add(node);

            if (node.IsGameOver || depth == MaxDepth)
            {
                return Sign[colour] * node.Value;
            }

            ProcessNode(node, depth);

            return node.Children.Aggregate(Minimum, (current, child) => Math.Max(current, -NegaMax(child, 1 - colour, depth + 1, nodesSearched)));
        }

        public static float AlphaBetaNegaMax(INode node, int colour, int depth, IList<INode> nodesSearched = null)
        {
            return AlphaBetaNegaMax(node, colour, depth, -InitialAlphaBeta, InitialAlphaBeta, nodesSearched);
        }

        private static float AlphaBetaNegaMax(INode node, int colour, int depth, float alpha, float beta, IList<INode> nodesSearched = null)
        {
            if (nodesSearched != null)
                nodesSearched.Add(node);

            if (node.IsGameOver || depth == MaxDepth)
                return Sign[colour] * node.Value;

            ProcessNode(node, depth);

            var bestScore = Minimum;

            var scores = new List<float>();

            foreach (var child in node.Children)
            {
                var hash = ZobristHash.Hash(child.GameState, colour == 0);
                if (DepthFirstSearch.TranspositionTable.ContainsKey(hash))
                {
                    scores.Add(DepthFirstSearch.TranspositionTable[hash]);
                    GameBehaviour.Transpositions++;
                }
                else
                {
                    var score = -AlphaBetaNegaMax(child, 1 - colour, depth + 1, -beta, -alpha, nodesSearched);
                    scores.Add(score);
					if (!DepthFirstSearch.TranspositionTable.ContainsKey(hash))
                    	DepthFirstSearch.TranspositionTable.Add(hash, score);
                }
            }

            foreach (var score in scores)
            {
                if (score >= beta)
                    return score;

                if (score > bestScore)
                    bestScore = score;

                if (score > alpha)
                    alpha = score;
            }
            return bestScore;
        }

        private static IEnumerable<INode> FilterOutTranspositions(IEnumerable<INode> children, int colour, out List<float> scores)
        {
            scores = new List<float>();

            if (DepthFirstSearch.TranspositionTable == null)
                return children;

            var childrenToSearch = new List<INode>();

            foreach (var child in children)
            {
                var hash = ZobristHash.Hash(child.GameState, colour == 0);
                if (DepthFirstSearch.TranspositionTable.ContainsKey(hash))
                {
                    scores.Add(DepthFirstSearch.TranspositionTable[hash]);
                    GameBehaviour.Transpositions++;
                }
                else
                {
                    childrenToSearch.Add(child);
                }
            }

            return childrenToSearch;
        }

        public static float NegaScout(INode node, int colour, int depth, IList<INode> nodesSearched = null)
        {
            return NegaScout(ref node, colour, depth, -InitialAlphaBeta, InitialAlphaBeta, nodesSearched);
        }

        private static float NegaScout(ref INode node, int colour, int depth, float alpha, float beta, IList<INode> nodesSearched = null)
        {
            if (nodesSearched != null)
                nodesSearched.Add(node);

            if (node.IsGameOver || depth == MaxDepth)
                return Sign[colour] * node.Value;

            ProcessNode(node, depth);

            // Order children so NegaScout can search effectively
            var orderedChildren = node.Children.OrderByDescending(x => x.Value);

            var b = beta;

            var firstChildSearched = false;
            foreach (var child in orderedChildren)
            {
                var c = child;
                var t = -NegaScout(ref c, 1 - colour, depth + 1, -b, -alpha, nodesSearched);
                if ((t > alpha) && (t < beta) && firstChildSearched)
                    t = -NegaScout(ref c, 1 - colour, depth + 1, -beta, -alpha, nodesSearched);

                alpha = Math.Max(alpha, t);

                if (alpha >= beta)
                    return alpha;

                b = alpha + 1;

                firstChildSearched = true;
            }
            return alpha;
        }


        private static void ProcessNode(INode node, int depth)
        {
            if (depth > MaxDepth)
                throw new ApplicationException(string.Format("Attempting to search (depth {0}) below maximum depth ({1})", depth, MaxDepth));

            // If the current node doesn't have any children, we must need to skip a turn.
            if (!node.HasChildren)
            {
                node.NextTurn();
            }
        }
    }
}