using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JSONExport : MonoBehaviour
{
	public void GenerateJSON()
	{
		var result = new StringBuilder();

		foreach (Transform plane in transform)
		{
			var colliders = plane.GetComponents<PolygonCollider2D>();

			foreach(var collider in colliders)
			{
				var points = collider.points;

				result.AppendFormat("{0}:\r\n[\r\n", plane.name);
				foreach (var point in points)
					result.AppendFormat("    [{0}, {1}],\r\n", point.x.ToString().Replace(",", "."), point.y.ToString().Replace(",", "."));
				result.Append("],");
			}

			print(result.ToString());
		}

		File.WriteAllText(Path.Combine(Application.persistentDataPath, "boundaries.txt"), result.ToString());
	}
}
