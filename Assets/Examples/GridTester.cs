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
		var limitSqrDist = gizmoDistance * gizmoDistance;
		var count = hashGrid.PointCount();
		for (var id = 0; id < count; id++) {
			var t = hashGrid.GetTransform(id);

			Gizmos.color = Color.yellow;
			switch (debugMode) {
			default:
				break;
			case DebugModeEnum.Distance:
				foreach (var n in hashGrid.Neighbors(id)) {
					if (id < n.id && n.sqrDistance < limitSqrDist)
						Gizmos.DrawLine(t.position, n.point.position);
				}
				break;
			case DebugModeEnum.Nearest:
				HashGrid2D.Neighbor nearest;
				if (hashGrid.Nearest(id, out nearest))
					Gizmos.DrawLine(t.position, nearest.point.position);
				break;
			}
		}
	}
}
