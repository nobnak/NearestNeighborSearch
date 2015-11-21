using UnityEngine;
using System.Collections.Generic;

namespace NearestNeighborSearch {

	public class HashGrid2D : MonoBehaviour {
		public GizmoDrawer debug;
		public Transform targetSpace;
		public float cellSize = 1f;
		public int hashSize = 37;

		bool _built = false;
		Cell[] _cells;
		List<Node> _points;

		void Awake() {
			_points = new List<Node>();
		}
		void OnDrawGizmos() {
			if (_built)
				debug.DrawGizmos (this);
		}

		public Cell[] Cells { get { return _cells; } }
		public List<Node> Points { get { return _points; } }

		public void Add(Transform p) {
			_built = false;
			_points.Add(new Node(p));
		}
		public void Clear() {
			_built = false;
			_points.Clear();
		}
		public void Build() {
			_built = true;
			var pointCount = _points.Count;
			for (var i = 0; i < pointCount; i++)
				_points[i].Update(Hash);
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
		public IEnumerable<Neighbor> Find(Vector2 center) {
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
							yield return new Neighbor(id1, p, sqrDist);
					}
				}
			}
		}

		void Discretize(Vector2 p, out int x, out int y) {
			x = Repeat((int)(p.x / cellSize));
			y = Repeat((int)(p.y / cellSize));
		}
		int Hash(int x, int y) {
			return x + hashSize * y;
		}
		int Hash(Vector2 p) {
			int x, y;
			Discretize (p, out x, out y);
			return Hash(x, y);
		}
		void Unhash(int hash, out int x, out int y) {
			y = hash / hashSize;
			x = hash - y * hashSize;
		}
		int Repeat(int x) {
			if (x < 0)
				return x - ((x + 1) / hashSize - 1) * hashSize;
			else
				return x - (x / hashSize) * hashSize;
		}


		public class Node : System.IComparable<Node> {
			public int cellId;
			public Vector2 position;
			public Transform point;

			public Node(Transform point) {
				this.cellId = -1;
				this.position = Vector2.zero;
				this.point = point;
			}
			public void Update(System.Func<Vector2, int> Hash) {
				position = (Vector2)point.position;
				cellId = Hash (position);
			}

			#region IComparable implementation
			public int CompareTo (Node other) {
				if (other == null)
					return -1;
				return cellId - other.cellId;
			}
			#endregion
		}
		public struct Cell {
			public int startIndex;
			public int length;

			public Cell(int startIndex, int length) {
				this.startIndex = startIndex;
				this.length = length;
			}
		}

		public struct Neighbor : System.IComparable<Neighbor> {
			public readonly int id;
			public readonly float sqrDistance;
			public readonly Node node;

			public Neighbor(int id, Node node, float sqrDistance) {
				this.id = id;
				this.node = node;
				this.sqrDistance = sqrDistance;
			}

			#region IComparable implementation
			public int CompareTo (Neighbor other) {
				var diff = sqrDistance - other.sqrDistance;
				return diff < 0 ? -1 : (diff > 0 ? +1 : 0);
			}
			#endregion
		}

		[System.Serializable]
		public class GizmoDrawer {
			public enum DebugModeEnum { Normal = 0, Distance, Nearest }

			public DebugModeEnum debugMode;

			public void DrawGizmos(HashGrid2D hashGrid) {
				if (debugMode == DebugModeEnum.Normal)
					return;

				Gizmos.color = Color.yellow;

				var points = hashGrid.Points;
				var pointCount = points.Count;
				for (var i = 0; i < pointCount; i++) {
					var p = points [i];
					var neighbors = new SortedList<float, Neighbor> ();
					foreach (var n in hashGrid.Find(p.position))
						if (p != n.node)
							neighbors.Add(n.sqrDistance, n);

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