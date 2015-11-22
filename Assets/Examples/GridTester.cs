using UnityEngine;
using System.Collections;
using NearestNeighborSearch;
using System.Collections.Generic;

public class GridTester : MonoBehaviour {
	public BaseHashGrid hashGrid;
	public GameObject pointfab;
	public Vector3 angularSpeed;
	public float radius = 1f;
	public int count = 1;

	void Start () {
		for (var i = 0; i < count; i++) {
			var p = Instantiate(pointfab);
			p.transform.SetParent(transform);
			p.transform.localPosition = radius * Random.insideUnitSphere;
			hashGrid.Add(p.transform);
		}
		//StartCoroutine(ContinuousBuilder(1f));
	}
	void Update () {
		transform.localRotation *= Quaternion.Euler(angularSpeed * Time.deltaTime);
		hashGrid.Build ();
	}

	IEnumerator ContinuousBuilder(float interval) {
		while (true) {
			yield return new WaitForSeconds(interval);
			hashGrid.Build();
		}
	}
}
