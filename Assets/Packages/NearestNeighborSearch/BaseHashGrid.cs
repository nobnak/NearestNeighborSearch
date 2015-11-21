using UnityEngine;
using System.Collections.Generic;

namespace NearestNeighborSearch {

	public abstract class BaseHashGrid : MonoBehaviour {
		public Transform targetSpace;
		public float cellSize = 1f;
		public int hashSize = 37;
		
		protected Cell[] _cells;

		public Cell[] Cells { get { return _cells; } }

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
	}
}