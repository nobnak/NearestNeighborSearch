using UnityEngine;
using System.Collections.Generic;

namespace NearestNeighborSearch {

	public abstract class BaseHashGrid : MonoBehaviour {
		public Transform targetSpace;
		public float cellSize = 1f;
		public int hashSize = 37;
		
		protected Cell[] _cells;

		public Cell[] Cells { get { return _cells; } }
		public Vector3 World2Local(Vector3 posWorld) {
			return targetSpace.InverseTransformPoint (posWorld);
		}
		public Vector3 Local2World(Vector3 posLocal) {
			return targetSpace.TransformPoint (posLocal);
		}

		public abstract void Add (Transform p);
		public abstract void Clear ();
		public abstract void Build();

		protected void Discretize(Vector2 p, out int x, out int y) {
			x = Repeat((int)(p.x / cellSize));
			y = Repeat((int)(p.y / cellSize));
		}
		protected void Discretize(Vector3 p, out int x, out int y, out int z) {
			x = Repeat((int)(p.x / cellSize));
			y = Repeat((int)(p.y / cellSize));
			z = Repeat((int)(p.z / cellSize));
		}
		protected int Hash(int x, int y) {
			return x + hashSize * y;
		}
		protected int Hash(int x, int y, int z) {
			return x + hashSize * (y + hashSize * z);
		}
		protected int Hash(Vector2 p) {
			int x, y;
			Discretize (p, out x, out y);
			return Hash(x, y);
		}
		protected int Hash(Vector3 p) {
			int x, y, z;
			Discretize (p, out x, out y, out z);
			return Hash(x, y, z);
		}
		protected void Unhash(int hash, out int x, out int y) {
			y = hash / hashSize;
			x = hash - y * hashSize;
		}
		protected void Unhash(int hash, out int x, out int y, out int z) {
			z = hash / (hashSize * hashSize);
			y = hash - z * (hashSize * hashSize);
			x = hash - y * hashSize;
		}
		protected int Repeat(int x) {
			if (x < 0)
				return x - ((x + 1) / hashSize - 1) * hashSize;
			else
				return x - (x / hashSize) * hashSize;
		}

		public struct Cell {
			public int startIndex;
			public int length;
			
			public Cell(int startIndex, int length) {
				this.startIndex = startIndex;
				this.length = length;
			}
		}		
		public class Node<Vec> : System.IComparable<Node<Vec>> {
			public int cellId;
			public Vec position;
			public Transform point;
			
			public Node(Transform point) {
				this.cellId = -1;
				this.position = default(Vec);
				this.point = point;
			}
			public void Update(Vec position, int cellId) {
				this.position = position;
				this.cellId = cellId;
			}
			
			#region IComparable implementation
			public int CompareTo (Node<Vec> other) {
				if (other == null)
					return -1;
				return cellId - other.cellId;
			}
			#endregion
		}		
		public struct Neighbor<Vec> : System.IComparable<Neighbor<Vec>> {
			public readonly int id;
			public readonly float sqrDistance;
			public readonly Node<Vec> node;
			
			public Neighbor(int id, Node<Vec> node, float sqrDistance) {
				this.id = id;
				this.node = node;
				this.sqrDistance = sqrDistance;
			}
			
			#region IComparable implementation
			public int CompareTo (Neighbor<Vec> other) {
				var diff = sqrDistance - other.sqrDistance;
				return diff < 0 ? -1 : (diff > 0 ? +1 : 0);
			}
			#endregion
		}
	}
}