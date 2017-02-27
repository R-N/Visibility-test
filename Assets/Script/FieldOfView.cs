using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;



public class FieldOfView : MonoBehaviour {
	public Transform trans = null;
	public Transform parentTrans = null;
	Quaternion defRot = Quaternion.identity;
	public LayerMask visibleMaskExceptLP;
	public LayerMask visibleMask;
	public Camera cam = null;
	public Camera cam2 = null;
	public Camera cam3 = null;
	public Camera cam4 = null;
	public Camera cam5 = null;
	public Camera cam6 = null;
	CommandBuffer buffer = null;
	public Shader shad = null;
	Material mat = null;
	public Material mat2 = null;

	public MeshFilter maskPizzaFilter = null;
	public MeshFilter maskCircleFilter = null;
	public MeshFilter maskPizzaFilter2 = null;
	public MeshFilter maskCircleFilter2 = null;

	public Renderer maskPizzaRend = null;
	public Renderer maskCircleRend = null;
	public Renderer maskPizzaRend2 = null;
	public Renderer maskCircleRend2 = null;

	public bool mist = false;

	RenderTargetIdentifier rtid1;
	RenderTargetIdentifier rtid2;

	public BlurOptimized m_blur;


	int angle = 60;
	int halfAngle = 30;
	float sightRange = 15;
	float localSightRange = 7.5f;
	float hearRange = 7.5f;
	float localHearRange = 3.75f;
	float scale = 2;
	float onePerScale = 0.5f;

	public static int arenaLayer = 0;
	public static int visibleArenaLayer = 0;
	public static int hiddenPlayerLayer = 0;
	public static int visiblePlayerLayer = 0;

	Vector3[] arrayPizzaVert0 = new Vector3[61];
	Vector3[] arrayCircleVert0 = new Vector3[361];
	Vector2[] arrayPizzaUV0 = new Vector2[61];
	Vector2[] arrayCircleUV0 = new Vector2[361];

	Vector3[] arrayPizzaVert = new Vector3[61];
	Vector3[] arrayCircleVert = new Vector3[361];
	Vector2[] arrayPizzaUV = new Vector2[61];
	Vector2[] arrayCircleUV = new Vector2[361];

	Vector3[] arrayPizzaVert2 = new Vector3[61];
	Vector3[] arrayCircleVert2 = new Vector3[361];
	Vector2[] arrayPizzaUV2 = new Vector2[61];
	Vector2[] arrayCircleUV2 = new Vector2[361];

	Vector3[] vertDir = new Vector3[361];
	Vector2[] uvNorm = new Vector2[361];

	RaycastHit[] hits = new RaycastHit[25];

	Vector3 myPos = Vector3.zero;
	//public RenderTexture rt = null;

	// Use this for initialization
	void Awake () {

		arenaLayer = LayerMask.NameToLayer ("Arena");
		visibleArenaLayer = LayerMask.NameToLayer ("VisibleArena");
		hiddenPlayerLayer = LayerMask.NameToLayer ("HiddenPlayer");
		visiblePlayerLayer = LayerMask.NameToLayer ("VisiblePlayer");


		trans = transform;
		defRot = trans.rotation;
		parentTrans = trans.root;
		mat = new Material( shad );
		buffer = new CommandBuffer();

		cam2.targetTexture = new RenderTexture (Screen.width, Screen.height, 16);
		cam3.targetTexture = new RenderTexture (Screen.width, Screen.height, 0);
		cam4.targetTexture = new RenderTexture (Screen.width, Screen.height, 0);
		cam5.targetTexture = new RenderTexture (Screen.width, Screen.height, 0);
		cam6.targetTexture = new RenderTexture (Screen.width, Screen.height, 16);

		rtid1 = new RenderTargetIdentifier(cam3.targetTexture);
		rtid2 = new RenderTargetIdentifier(cam4.targetTexture);

		List<Vector3> vertsCircle = new List<Vector3> ();
		List<Vector3> vertsPizza = new List<Vector3> ();
		List<Vector2> uvCircle = new List<Vector2> ();
		List<Vector2> uvPizza = new List<Vector2> ();
		List<int> trisPizza = new List<int> ();
		List<int> trisCircle = new List<int> ();

		vertsPizza.Add (Vector3.zero);
		vertsCircle.Add (Vector3.zero);
		uvPizza.Add (Vector2.one * 0.5f);
		uvCircle.Add (Vector2.one * 0.5f);


		int n = 1;
		for (int a = 90 - halfAngle; a < 450 - halfAngle; a++) {
			Vector2 raw = new Vector2(Mathf.Cos (Mathf.Deg2Rad * a), Mathf.Sin(Mathf.Deg2Rad * a));
			if (n < 61) {
				vertsPizza.Add (new Vector3 (raw.x * localSightRange, 0, raw.y * localSightRange));
				if (n < 60) {
					trisPizza.Add (0);
					trisPizza.Add (n + 1);
					trisPizza.Add (n);
				}
				uvPizza.Add ((raw + Vector2.one) * 0.5f);
			}
			vertsCircle.Add (new Vector3 (raw.x * localHearRange, 0, raw.y * localHearRange));

			uvCircle.Add ((raw + Vector2.one) * 0.5f);
			vertDir [n] = new Vector3 (raw.x, 0, raw.y);
			uvNorm [n] = raw;
			if (n == 360) {
				trisCircle.Add (0);
				trisCircle.Add (1);
				trisCircle.Add (360);
			} else {
				trisCircle.Add (0);
				trisCircle.Add (n + 1);
				trisCircle.Add (n);
			}
			n++;
		}
		arrayPizzaVert0 = vertsPizza.ToArray ();
		arrayCircleVert0 = vertsCircle.ToArray ();
		arrayCircleUV0 = uvCircle.ToArray ();
		arrayPizzaUV0 = uvPizza.ToArray ();

		arrayPizzaVert = vertsPizza.ToArray ();
		arrayCircleVert = vertsCircle.ToArray ();
		arrayCircleUV = uvCircle.ToArray ();
		arrayPizzaUV = uvPizza.ToArray ();

		arrayPizzaVert2 = vertsPizza.ToArray ();
		arrayCircleVert2 = vertsCircle.ToArray ();
		arrayCircleUV2 = uvCircle.ToArray ();
		arrayPizzaUV2 = uvPizza.ToArray ();

		maskPizzaFilter.mesh = new Mesh ();
		maskPizzaFilter.mesh.vertices = arrayPizzaVert;
		maskPizzaFilter.mesh.uv = arrayPizzaUV;
		maskPizzaFilter.mesh.triangles = trisPizza.ToArray ();

		maskCircleFilter.mesh = new Mesh ();
		maskCircleFilter.mesh.vertices = arrayCircleVert;
		maskCircleFilter.mesh.uv = arrayCircleUV;
		maskCircleFilter.mesh.triangles = trisCircle.ToArray ();

		maskPizzaFilter2.mesh = new Mesh ();
		maskPizzaFilter2.mesh.vertices = arrayPizzaVert2;
		maskPizzaFilter2.mesh.uv = arrayPizzaUV2;
		maskPizzaFilter2.mesh.triangles = trisPizza.ToArray ();

		maskCircleFilter2.mesh = new Mesh ();
		maskCircleFilter2.mesh.vertices = arrayCircleVert2;
		maskCircleFilter2.mesh.uv = arrayCircleUV2;
		maskCircleFilter2.mesh.triangles = trisCircle.ToArray ();


	}

	float SqDist(Vector3 a, Vector3 b){
		Vector3 c = b - a;
		return c.x * c.x + c.y * c.y + c.z * c.z;
	}

	void RecursiveChangeLayer(Transform t, int layer){
		t.gameObject.layer = layer;
		foreach (Transform c in t) {
			RecursiveChangeLayer (c, layer);
		}
	}

	void LateUpdate(){

		trans.rotation = defRot;

		myPos = parentTrans.position;

		/*Collider[] cols = Physics.OverlapSphere (parentTrans.position, sightRange, visibleMaskExceptLP);

		foreach (Collider a in cols) {
			Transform t = a.transform;
			Vector3 delta = t.position - myPos;
			float dist = delta.magnitude;
			delta /= dist;

			if (dist <= hearRange) {
				GameObject go = a.gameObject;

				 if (go.layer == hiddenPlayerLayer) {
					RecursiveChangeLayer (t.root, visiblePlayerLayer);
				}

			} else {
				RaycastHit hit;
				if (Physics.Raycast (myPos, delta, out hit, sightRange, visibleMaskExceptLP)
					&& hit.collider == a && Vector3.Dot(parentTrans.forward, delta) >= 0.85f) {


					GameObject go = a.gameObject;

					 if (go.layer == hiddenPlayerLayer) {
						RecursiveChangeLayer (t.root, visiblePlayerLayer);
					}
				}
			}
		}*/


		for (int n = 1; n < 61; n++){
			//arrayVert[n] = (worldToLocal  * RayCheck (parentTrans.rotation * vertDir [n])) * 1.5f;
			bool isPlayer = true;
			float distToObj = localSightRange;
			//if (n < 61) {
			arrayPizzaVert [n] = Quaternion.Inverse (parentTrans.rotation) * (RayCheck (parentTrans.rotation * vertDir [n], sightRange, out isPlayer, out distToObj) - parentTrans.position) * onePerScale;

			if (isPlayer) {
				float mag = arrayPizzaVert [n].magnitude;
				float mag2 = Mathf.Clamp ((mag + distToObj) * 0.5f, 0, localSightRange);
				arrayPizzaVert [n] = arrayPizzaVert [n] * mag2 / mag;
				arrayPizzaUV [n] = (uvNorm [n] * mag2 / localSightRange + Vector2.one) * 0.5f;

				/*if (isPlayer) {
					arrayPizzaVert2 [n] = arrayPizzaVert0 [n];
					arrayPizzaUV2 [n] = arrayPizzaUV0 [n];
				} else {*/
			} else {
				arrayPizzaUV [n] = (uvNorm [n] * arrayPizzaVert [n].magnitude / localSightRange + Vector2.one) * 0.5f;
			}
			/*arrayPizzaVert2 [n] = arrayPizzaVert [n];
			arrayPizzaUV2 [n] = arrayPizzaUV [n];*/
				//}

			//} 

			/*Vector3 result = RayAllCheck (parentTrans.rotation * vertDir [n], hearRange, out isPlayer);
			if (isPlayer){
				arrayCircleVert [n] = arrayCircleVert0 [n];
				arrayCircleUV [n] = arrayCircleUV0 [n];
				arrayCircleVert2 [n] = arrayCircleVert0 [n];
				arrayCircleUV2 [n] = arrayCircleUV0 [n];
			}else{
				arrayCircleVert [n] = Quaternion.Inverse (parentTrans.rotation) * (result - parentTrans.position) * onePerScale;
				arrayCircleUV [n] = (uvNorm [n] * arrayCircleVert [n].magnitude / localHearRange + Vector2.one) * 0.5f;
				arrayCircleVert2 [n] = arrayCircleVert [n];
				arrayCircleUV2 [n] = arrayCircleUV [n];
			}*/

		}

		maskPizzaFilter.mesh.vertices = arrayPizzaVert;
		maskPizzaFilter.mesh.uv = arrayPizzaUV;
		maskPizzaFilter.mesh.RecalculateNormals ();

		/*maskCircleFilter.mesh.vertices = arrayCircleVert;
		maskCircleFilter.mesh.uv = arrayCircleUV;
		maskCircleFilter.mesh.RecalculateNormals ();*/

		/*maskPizzaFilter2.mesh.vertices = arrayPizzaVert2;
		maskPizzaFilter2.mesh.uv = arrayPizzaUV2;
		maskPizzaFilter2.mesh.RecalculateNormals ();*/

		/*maskCircleFilter2.mesh.vertices = arrayCircleVert2;
		maskCircleFilter2.mesh.uv = arrayCircleUV2;
		maskCircleFilter2.mesh.RecalculateNormals ();*/

		RenderTexture.active = null;

	}

	Vector3 ClampMagnitude(Vector3 a, float min, float max){
		float mag = a.magnitude;
		return a * Mathf.Clamp (mag, min, max) / mag;
	}

	Vector3 ClampMinMag(Vector3 a, float min){
		float mag = a.magnitude;
		if (mag < min) {
			return a * min / mag;
		}
		return a;
	}

	Vector3 RayCheck(Vector3 dir, float range, out bool isPlayer, out float dist){
		RaycastHit hit;
		isPlayer = true;
		dist = localSightRange;
		if (Physics.Raycast (myPos, dir, out hit, range, visibleMaskExceptLP)) {
			Transform t = hit.transform;
			GameObject go = hit.collider.gameObject;

			/*if (go.layer == hiddenPlayerLayer) {
				RecursiveChangeLayer (t.root, visiblePlayerLayer);
			}else*/ if (go.layer != visiblePlayerLayer){
				isPlayer = false;
			}

			dist = Vector3.Distance (t.position, parentTrans.position) * onePerScale;

			return hit.point;
		} else {
			return parentTrans.position + dir * range;
		}
	}



	Vector3 RayAllCheck(Vector3 dir, float range, out bool isPlayer){
		isPlayer = true;
		//RaycastHit[] hits = Physics.RaycastAll (myPos, dir, range, visibleMaskExceptLP);
		int m = Physics.RaycastNonAlloc(myPos, dir, hits, range, visibleMaskExceptLP, QueryTriggerInteraction.Ignore);
		if (m > 0){
			Vector3 ret = parentTrans.position + dir * range;
			Vector3 hitWallPos = Vector3.zero;
			float hitWallDist = Mathf.Infinity;
			for (int a = 0; a < m; a++){
				Transform t = hits[a].transform;
				GameObject go = t.gameObject;

				if (go.layer == hiddenPlayerLayer) {
					if (float.IsInfinity (hitWallDist) || SqDist(myPos, t.position) <= hitWallDist) {
						RecursiveChangeLayer (t.root, visiblePlayerLayer);
						ret = hits [a].point;
					}
				}else if (go.layer != visiblePlayerLayer){
					float dist = SqDist (myPos, hits [a].point);
					if (dist < hitWallDist) {
						hitWallDist = dist;
						hitWallPos = hits [a].point;
					}
				}
			}
			if (!float.IsInfinity(hitWallDist)) {
				isPlayer = false;
				return hitWallPos;;
			} else {
				return ret;
			}
		} else {
			return parentTrans.position + dir * range;
		}
	}


	void OnRenderImage(RenderTexture source, RenderTexture destination ){

		RenderTexture.active = cam2.targetTexture;
		GL.Clear (true, true, Color.clear);
		cam2.Render ();

		/*cam.cullingMask = sightMask;
		buffer.Clear ();*/

		/*RenderTargetIdentifier rtid = new RenderTargetIdentifier(rt);
		buffer.SetRenderTarget( rtid );
		buffer.DrawRenderer (mask, mat, 0, 0);
		Graphics.ExecuteCommandBuffer(buffer);*/


		RenderTexture.active = cam3.targetTexture;
		GL.Clear (true, true, Color.clear);
		cam3.Render ();
		m_blur.OnRenderImage (cam3.targetTexture, cam3.targetTexture);
		/*buffer.Clear();
		buffer.SetRenderTarget (rtid1);
		buffer.DrawRenderer (maskCircleRend, mat2, 0, 0);
		buffer.DrawRenderer (maskPizzaRend, mat2, 0, 0);
		Graphics.ExecuteCommandBuffer(buffer);*/

		/*RenderTexture.active = cam4.targetTexture;
		GL.Clear (true, true, Color.clear);
		cam4.Render ();*/
		/*buffer.Clear();
		buffer.SetRenderTarget (rtid2);
		buffer.DrawRenderer (maskCircleRend2, mat2, 0, 0);
		buffer.DrawRenderer (maskPizzaRend2, mat2, 0, 0);
		Graphics.ExecuteCommandBuffer(buffer);*/

		RenderTexture.active = cam6.targetTexture;
		GL.Clear (true, true, Color.clear);
		cam6.Render ();

		mat.SetTexture ("_ArenaTex", cam6.targetTexture);
		//mat.SetTexture ("_VisibleMask", cam4.targetTexture);
		mat.SetTexture ("_Mask", cam3.targetTexture);
		mat.SetTexture ("_VisibleTex", cam2.targetTexture);

		if (mist) {
			RenderTexture.active = cam5.targetTexture;
			GL.Clear (true, true, Color.clear);
			cam5.Render ();

			mat.SetTexture ("_Mist", cam5.targetTexture);
			Graphics.Blit (source, destination, mat, 2);
		} else {
			Graphics.Blit (source, destination, mat, 1);
		}
		RenderTexture.active = null;
	}
}
