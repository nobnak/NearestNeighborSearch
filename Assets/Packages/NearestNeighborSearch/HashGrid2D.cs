using UnityEngine;
using System.Collections.Generic;

namespace NearestNeighborSearch {

	public class HashGrid2D : BaseHashGrid {
		public GizmoDrawer debug;

		bool _built = false;
		List<Node<Vector2>> _points;

		void Awake() {
			_points = new List<Node<Vector2>>();
		}
		void OnDrawGizmos() {
			if (_built)
				debug.DrawGizmos (this);
		}

		public List<Node<Vector2>> Points { get { return _points; } }

		public override void Add(Transform p) {
			_built = false;
			_points.Add(new Node<Vector2>(p));
		}
		public override void Clear() {
			_built = false;
			_points.Clear();
		}
		public override void Build() {
			_built = true;
			var pointCount = _points.Count;
			for (var i = 0; i < pointCount; i++) {
				var p = _points[i];
				var pos = (Vector2)World2Local(p.point.position);
				p.Update(pos, Hash(pos));
			}
			_points.Sort();

			var cellCount = hashSize * hashSize;
			if (_cells == null || _cells.Length != cellCount)
				_cells = new Cell[cellCount];
			System.Array.Clear (_cells, 0, _cells.Length);

			if (pointCount == 0)
				return;
			var start = _points [0];
			var curr = start;
			var offset = 0;
			var count = 1;
			for (var i = 1; i < pointCount; i++) {
				curr = _points[i];
				if (start.cellId != curr.cellId) {
					_cells[start.cellId] = new Cell(offset, count);
					offset = i;
					count = 1;
					start = curr;
				} else {
					count++;
				}
			}
			_cells [start.cellId] = new Cell (offset, count);
		}
		public IEnumerable<Neighbor<Vector2>> Find(Vector2 center) {
			var limitSqrDist = 2f * cellSize * cellSize;
			int x, y;
			Discretize(center, out x, out y);
			for (var dy = -1; dy <= 1; dy++) {
				for (var dx = -1; dx <= 1; dx++) {
					var id0 = Hash(Repeat(x + dx), Repeat(y + dy));
					var cell = _cells[id0];
					for (var i = 0; i < cell.length; i++) {
						var id1 = i + cell.startIndex;
						var p = _points[id1];
						var path = p.position - center;
						var sqrDist = path.sqrMagnitude;
						if (sqrDist < limitSqrDist)
							yield return new Neighbor<Vector2>(id1, p, sqrDist);
					}
				}
			}
		}

		[System.Serializable]
		public class GizmoDrawer {
			public enum DebugModeEnum { Normal = 0, Distance, Nearest }

			public DebugModeEnum debugMode;

			public void DrawGizmos(HashGrid2D hashGrid) {
				if (debugMode == DebugModeEnum.Normal)
					return;

				var c0 = Color.green;
				var c1 = Color.red;
				var limitCount = 9;

				var points = hashGrid.Points;
				var pointCount = points.Count;
				for (var i = 0; i < pointCount; i++) {
					var p = points [i];
					var neighbors = new SortedList<float, Neighbor<Vector2>> ();
					foreach (var n in hashGrid.Find(p.position))
						if (p != n.node)
							neighbors.Add(n.sqrDistance, n);
					
					var neighborCount = neighbors.Count;
					Gizmos.color = Color.Lerp(c0, c1, (float)(neighborCount - 1) / limitCount);

					switch (debugMode) {
					case DebugModeEnum.Distance:
						foreach (var n in neighbors)
							Gizmos.DrawLine(p.point.position, n.Value.node.point.position);
						break;
					case DebugModeEnum.Nearest:
						if (neighbors.Count > 0) {
							var n = neighbors.Values[0];
							Gizmos.DrawLine(p.point.position, n.node.point.position);
						}
						break;
					}
				}
			}
		}
	}
}