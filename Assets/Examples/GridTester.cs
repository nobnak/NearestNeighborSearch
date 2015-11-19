using UnityEngine;
using System.Collections;
using NearestNeighborSearch;
using System.Collections.Generic;

public class GridTester : MonoBehaviour {
	public HashGrid2D hashGrid;
	public GameObject pointfab;
	public Vector3 angularSpeed;
	public float radius = 1f;
	public int count = 1;

	public enum DebugModeEnum { Normal, Distance, Nearest }
	public float gizmoDistance = 1f;
	public DebugModeEnum debugMode;

	void Start () {
		for (var i = 0; i < count; i++) {
			var p = Instantiate(pointfab);
			p.transform.SetParent(transform);
			p.transform.localPosition = radius * Random.insideUnitSphere;
			hashGrid.Add(p.transform);
		}
	}
	void Update () {
		transform.localRotation *= Quaternion.Euler(angularSpeed * Time.deltaTime);
		hashGrid.Build();
	}
	void OnDrawGizmos() {
		var d = gizmoDistance;

		var count = hashGrid.PointCount();
		for (var i = 0; i < count; i++) {
			var t = hashGrid.GetTransform(i);
			var p = hashGrid.GetPosition(i);
			var neighbors = new LinkedList<HashGrid2D.Neighbor>();
			var minSqrDist = float.MaxValue;
			HashGrid2D.Neighbor nearest;
			foreach (var n in hashGrid.Neighbors(p, d)) {
				if (i < n.id) {
					neighbors.AddLast(n);
					if (n.sqrDistance < minSqrDist) {
						minSqrDist = n.sqrDistance;
						nearest = n;
					}
				}
			}

			Gizmos.color = Color.yellow;
			switch (debugMode) {
			default:
				break;
			case DebugModeEnum.Distance:
				foreach (var n in neighbors)
					Gizmos.DrawLine(t.position, n.point.position);
				break;
			case DebugModeEnum.Nearest:
				if (neighbors.Count > 0)
					Gizmos.DrawLine(t.position, nearest.point.position);
				break;
			}
		}
	}
}
