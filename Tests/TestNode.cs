﻿using System;
using System.Collections.Generic;
using System.Linq;
using Reversi.Model;
using Reversi.Model.Evaluation;

namespace Tests
{
    class TestNode : INode
    {
        public TestNode(IEnumerable<INode> children)
        {
            Children = children;
        }

        public float Value { get; set; }

        public List<AnalysisNodeReference> ChildNodeReferences { get { throw new NotImplementedException(); } }
        public IEnumerable<INode> Children { get; private set; }
        public bool HasChildren { get { return Children.Any(); } }
        public bool IsGameOver { get { return false; } }
        public short? PlayIndex { get; set; }
        public GameState GameState { get; set; }

        public void NextTurn()
        {
            throw new NotImplementedException();
        }
    }
}
