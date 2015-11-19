using UnityEngine;
using System.Collections.Generic;

namespace NearestNeighborSearch {

	public class HashGrid2D : MonoBehaviour {
		public Transform targetSpace;
		public int cellCount = 37;
		public float cellSize = 1f;

		int _pointCount;
		float _gridSize;
		List<Transform> _points;
		List<Vector2> _positions;
		List<int>[,] _cells;

		void Awake() {
			_pointCount = 0;
			_points = new List<Transform>();
			_positions = new List<Vector2>();
		}

		public void Add(Transform p) {
			_pointCount++;
			_points.Add(p);
			_positions.Add(Vector2.zero);
		}
		public void Build() {
			if (_cells == null || _cells.GetLength(0) != cellCount || _cells.GetLength(1) != cellCount)
				InitGrid();
			ClearGrid();
			FillGrid();
		}
		public IEnumerable<Neighbor> Neighbors(int id0) {
			int x, y;
			var limitSqrDist = 2f * cellSize * cellSize;
			var p = _positions[id0];
			Hash(p, out x, out y);
			for (var dj = -1; dj <= 1; dj++) {
				for (var di = -1; di <= 1; di++) {
					var ix = Repeat(x + di);
					var iy = Repeat(y + dj);
					foreach (var id1 in _cells[ix, iy]) {
						if (id0 == id1)
							continue;
						var q = _positions[id1];
						var sqrDist = (q - p).sqrMagnitude;
						if (sqrDist < limitSqrDist)
							yield return new Neighbor(id1, _points[id1], _positions[id1], sqrDist);
					}
				}
			}
		}
		public bool Nearest(int id, out Neighbor nearest) {
			var minSqrDist = float.MaxValue;
			var found = false;
			nearest = default(Neighbor);
			foreach (var n in Neighbors(id)) {
				if (n.sqrDistance < minSqrDist) {
					minSqrDist = n.sqrDistance;
					found = true;
					nearest = n;
				}
			}
			return found;
		}
		public int PointCount() { return _pointCount; }
		public Transform GetTransform(int id) { return _points[id]; }
		public Vector2 GetPosition(int id) { return _positions[id]; }

		void InitGrid () {
			_cells = new List<int>[cellCount, cellCount];
			for (var j = 0; j < cellCount; j++)
				for (var i = 0; i < cellCount; i++)
					_cells [i, j] = new List<int> ();
		}
		void ClearGrid () {
			for (var j = 0; j < cellCount; j++)
				for (var i = 0; i < cellCount; i++)
					_cells [i, j].Clear ();
		}
		void FillGrid() {
			_gridSize = cellCount * cellSize;
			for (var i = 0; i < _pointCount; i++) {
				int x, y;
				var pos = _positions [i] = (Vector2)targetSpace.InverseTransformPoint(_points [i].position);
				Hash (pos, out x, out y);
				_cells [x, y].Add(i);
			}
		}
		void Hash(Vector2 pos, out int ix, out int iy) {
			ix = (int)((pos.x - Mathf.FloorToInt(pos.x / _gridSize) * _gridSize) / cellSize);
			iy = (int)((pos.y - Mathf.FloorToInt(pos.y / _gridSize) * _gridSize) / cellSize);

			ix = Repeat(ix);
			iy = Repeat(iy);
		}
		int Repeat(int x) {
			while (x < 0)
				x += cellCount;
			while (x >= cellCount)
				x -= cellCount;
			return x;
		}

		public struct Neighbor {
			public readonly int id;
			public readonly float sqrDistance;
			public readonly Transform point;
			public readonly Vector2 position;

			public Neighbor(int id, Transform point, Vector2 position, float sqrDistance) {
				this.id = id;
				this.point = point;
				this.position = position;
				this.sqrDistance = sqrDistance;
			}
		}
	}
}