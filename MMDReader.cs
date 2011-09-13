using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


namespace MMD
{
	
	public class MaterialData
	{
		public float[] diffuseColor;
		public float alpha;
		public float specularity;
		public float[] specularColor;
		public float[] mirrorColor;
		public byte toonIndex;
		public byte edgeFlag;
		public uint faceVertCount;
		public char[] texturePath;
		
		public MaterialData()
		{
			//Debug.Log("New MaterialData");
			
			diffuseColor = new float[3];
			specularColor = new float[3];
			mirrorColor = new float[3];
			
			alpha = 1.0f;
			specularity = 1.0f;
			toonIndex = 0;
			edgeFlag = 0;
			faceVertCount = 0;
			
			texturePath = new char[20];
		}
	}
	
	public class BoneData
	{
		public char[] boneName;
		public uint parentBoneIndex;
		public uint tailPosBoneIndex;
		public byte boneType;
		public uint ikParentBoneIndex;
		public float[] boneHeadPos;
		
		public BoneData()
		{
			boneName = new char[20];
			
			parentBoneIndex = 0;
			tailPosBoneIndex = 0;
			boneType = 0;
			ikParentBoneIndex = 0;
			
			boneHeadPos = new float[3];
		}
	}
	
	public class MMDReader : MonoBehaviour
	{
		
		//private string textureFolder = "";
		
		// VertexData
		private uint vertexCount = 0;
		private Vector3[] vertexPositions;
		private Vector3[] vertexNormals;
		private Vector2[] vertexUVs;
		private BoneWeight[] boneWeights;
		
		// VertexIndexis
		private uint faceVertexCount = 0;
		private int[] faceIndex;
		
		// MaterialData
		private uint materialCount = 0;
		private MaterialData[] materials;
		
		// BoneData
		private uint boneCount = 0;
		private BoneData[] bones;
		
		
		
		public void ReadMMDDataNotSkinned(string filePath, string createFileName)
		{
			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(stream);
						
			// OutputLog FileLength
			//Debug.Log(stream.Length);
	
			// Read Header
			ReadModelHeader(reader);
	
			// ReadMeshVertex
			ReadMeshVertexData(reader);
	
			// Read VertexIndexes
			ReadVertexIndexes(reader);
	
			// Read Materials
			ReadMaterialData(reader);
			
			// Read BoneData
			ReadBoneData(reader);
	
			
			// Create Folder
			AssetDatabase.CreateFolder("Assets", createFileName);
						
			//GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			GameObject obj = new GameObject("MMDObject");
			// Add Component(Not Skining)
			MeshFilter mf = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
			MeshRenderer mr = obj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			// Add Component(Skinning)
			//SkinnedMeshRenderer smr = obj.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
	
	
			// Create Mesh
			Mesh meshData = new Mesh();
			//meshData.vertexCount = vertexCount;
			meshData.vertices = vertexPositions;
			meshData.normals = vertexNormals;
			meshData.uv = vertexUVs;
			meshData.boneWeights = boneWeights;
			meshData.triangles = faceIndex;
			meshData.name = "Mesh";

			
			// Add Mesh Asset
			AssetDatabase.CreateAsset(meshData, "Assets/" + createFileName + "/" + createFileName + "Mesh.asset");
			
			// Set SubMeshs
			meshData.subMeshCount = (int)materialCount;
			int IndexOffset = 0;
			for(int i = 0; i < materialCount; i++)
			{
				int[] subMeshIndexes = new int[materials[i].faceVertCount];
			
				for(int j = 0; j < materials[i].faceVertCount; j++)
				{
					subMeshIndexes[j] = faceIndex[j + IndexOffset];
				}
							
				meshData.SetTriangles(subMeshIndexes, i);
							
				IndexOffset += (int)materials[i].faceVertCount;
			}
			
										
			// Set Mesh
			mf.mesh = meshData;
			mf.sharedMesh = meshData;
			
			
			// Create Material Folder
			AssetDatabase.CreateFolder("Assets/" + createFileName, "Materials");
			
			// Create Material
			Material[] meshMaterial = new Material[materialCount];
			for(int i = 0; i < materialCount; i++)
			{
				MaterialData md = materials[i];
							
				//meshMaterial[i] = new Material(smr.material);
				meshMaterial[i] = new Material(Shader.Find("Specular"));
							
				meshMaterial[i].color = new Color(md.diffuseColor[0], md.diffuseColor[1], md.diffuseColor[2], md.alpha);
				meshMaterial[i].SetColor("_SpecColor", new Color(md.specularColor[0], md.specularColor[1], md.specularColor[2]));
				meshMaterial[i].SetFloat("_Shininess", md.specularity);
				meshMaterial[i].SetColor("_ReflectColor", new Color(md.mirrorColor[0], md.mirrorColor[1], md.mirrorColor[2]));
				
				string path = new string(md.texturePath);
				string[] splitPath = path.Split('.');
				if(splitPath.Length > 1)
				{
					Texture tex = Resources.Load(splitPath[0]) as Texture;
					meshMaterial[i].mainTexture = tex;
				}
				
				string materialPath = "Assets/" + createFileName +"/Materials/" + createFileName + "_Material" + i + ".mat";
				AssetDatabase.CreateAsset(meshMaterial[i], materialPath);
			}
			mr.materials = meshMaterial;
						
	
			UnityEngine.Object newPrefab = EditorUtility.CreateEmptyPrefab("Assets/" + createFileName + "/" + createFileName + ".prefab");
			EditorUtility.ReplacePrefab(obj, newPrefab);
			AssetDatabase.Refresh();
			DestroyImmediate(obj);
						
			if(reader != null)
			{
				reader.Close();
			}
			if(stream != null)
			{
				stream.Close();
			}
		}
		
		
		public void ReadMMDDataSkinned(string filePath, string createFileName)
		{
			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(stream);
						
			// OutputLog FileLength
			//Debug.Log(stream.Length);
	
			// Read Header
			ReadModelHeader(reader);
	
			// ReadMeshVertex
			ReadMeshVertexData(reader);
	
			// Read VertexIndexes
			ReadVertexIndexes(reader);
	
			// Read Materials
			ReadMaterialData(reader);
			
			// Read BoneData
			ReadBoneData(reader);
	
			// Create Folder
			AssetDatabase.CreateFolder("Assets", createFileName);

						
			//GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			GameObject obj = new GameObject("MMDObject");
			UnityEngine.Object newPrefab = EditorUtility.CreateEmptyPrefab("Assets/" + createFileName + "/" + createFileName +".prefab");

			// Add Component(Skinning)
			SkinnedMeshRenderer smr = obj.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
	
	
			// Create Mesh
			Mesh meshData = new Mesh();
			//meshData.vertexCount = vertexCount;
			meshData.vertices = vertexPositions;
			meshData.normals = vertexNormals;
			meshData.uv = vertexUVs;
			meshData.boneWeights = boneWeights;
			meshData.triangles = faceIndex;
			meshData.name = "Mesh";
			
			
			AssetDatabase.CreateAsset(meshData, "Assets/" + createFileName + "/" + createFileName + "Mesh.asset");
			
			// Set SubMeshs
			meshData.subMeshCount = (int)materialCount;
			int IndexOffset = 0;
			for(int i = 0; i < materialCount; i++)
			{
				int[] subMeshIndexes = new int[materials[i].faceVertCount];
			
				for(int j = 0; j < materials[i].faceVertCount; j++)
				{
					subMeshIndexes[j] = faceIndex[j + IndexOffset];
				}
							
				meshData.SetTriangles(subMeshIndexes, i);
							
				IndexOffset += (int)materials[i].faceVertCount;
			}
			
			
			// Create Material Folder
			AssetDatabase.CreateFolder("Assets/" + createFileName, "Materials");
			
			// Create Material
			Material[] meshMaterial = new Material[materialCount];
			for(int i = 0; i < materialCount; i++)
			{
				MaterialData md = materials[i];
							
				//meshMaterial[i] = new Material(smr.material);
				meshMaterial[i] = new Material(Shader.Find("Specular"));
							
				meshMaterial[i].color = new Color(md.diffuseColor[0], md.diffuseColor[1], md.diffuseColor[2], md.alpha);
				meshMaterial[i].SetColor("_SpecColor", new Color(md.specularColor[0], md.specularColor[1], md.specularColor[2]));
				meshMaterial[i].SetFloat("_Shininess", md.specularity);
				meshMaterial[i].SetColor("_ReflectColor", new Color(md.mirrorColor[0], md.mirrorColor[1], md.mirrorColor[2]));
				
				string path = new string(md.texturePath);
				string[] splitPath = path.Split('.');
				if(splitPath.Length > 1)
				{
					Texture tex = Resources.Load(splitPath[0]) as Texture;
					meshMaterial[i].mainTexture = tex;
				}
				
				// Create Material Asset
				string materialPath = "Assets/" + createFileName + "/Materials/" + createFileName + "_Material" + i + ".mat";
				AssetDatabase.CreateAsset(meshMaterial[i], materialPath);
			}
			//smr.materials = meshMaterial;
			
			
			// Create Bone Folder
			string boneFolder = AssetDatabase.CreateFolder("Assets/" + createFileName, "Bones");
			
			
			// Create Bones
			smr.bones = new Transform[boneCount];
			GameObject[] boneObject = new GameObject[boneCount];
			for(int i = 0; i < boneCount; i++)
			{				
				BoneData bone = bones[i];
				
				boneObject[i] = new GameObject("Bone" + i);
				//boneObject[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//boneObject[i].name = "Bone" + i;
				
				//boneObject[i].transform.position = new Vector3(bone.boneHeadPos[0], bone.boneHeadPos[1], bone.boneHeadPos[2]);
				boneObject[i].transform.position = new Vector3(bone.boneHeadPos[0], bone.boneHeadPos[1], bone.boneHeadPos[2]);
			}
			// Attatch Parent
			Transform[] trans = new Transform[boneCount];
			for(int i = 0; i < boneCount; i++)
			{
				BoneData bone = bones[i];
				
				if(bone.parentBoneIndex == 65535)
				{
					//boneObject[i].transform.parent = null;
					boneObject[i].transform.parent = obj.transform;
				}
				else
				{
					boneObject[i].transform.parent = boneObject[bone.parentBoneIndex].transform;
				}
				
				trans[i] = boneObject[i].transform;
			}
			// Create Mesh BindPose
			Matrix4x4[] bindPoses = new Matrix4x4[boneCount];
			for(int i = 0; i < boneCount; i++)
			{
				bindPoses[i] = boneObject[i].transform.worldToLocalMatrix;
			}
			meshData.bindposes = bindPoses;
			
			//AssetDatabase.CreateAsset(rootBone, "Assets/" + createFileName + "/Bones/Bone.prefab");
			
			// Set Mesh
			smr.sharedMesh = meshData;
			// Set Bones
			smr.bones = trans;
			// Set Materials
			smr.materials = meshMaterial;


			EditorUtility.ReplacePrefab(obj, newPrefab);
			AssetDatabase.Refresh();
			DestroyImmediate(obj);
						
			if(reader != null)
			{
				reader.Close();
			}
			if(stream != null)
			{
				stream.Close();
			}
		}
		
	
		
		
		private void ReadPluralFloat(BinaryReader reader, float[] value, uint count)
		{
			for(int i = 0; i < count; i++)
			{
				value[i] = reader.ReadSingle();
			}
		}
		
		
		private void ReadModelHeader(BinaryReader reader)
		{
			// Read Header
			char[] magic = new char[3];
			magic = reader.ReadChars(3);
			string magicStr = new string(magic);
			//Debug.Log("Magic Code:" + magicStr);
			
			//Debug.Log(reader.BaseStream.Position);
			
			// Read Vertsion Info
			float version = reader.ReadSingle();
			//Debug.Log("Version:" + version);
			
			//Debug.Log(reader.BaseStream.Position);
			
			// Read ModelName
			char[] modelName = new char[20];
			//modelName = reader.ReadChars(20);
			for(int i = 0; i < 20; i++)
			{
				modelName[i] = reader.ReadChar();
			}
			string modelNameStr = new string(modelName);
			//Debug.Log("modelName:" + modelNameStr);
			
			//Debug.Log(reader.BaseStream.Position);
			reader.BaseStream.Seek(27, SeekOrigin.Begin);
		
			// Read Comment
			char[] comment = new char[256];
			//comment = reader.ReadChars(256);
			for(int i = 0; i < 256; i++)
			{
				comment[i] = reader.ReadChar();
			}
			string commentStr = new string(comment);
			//Debug.Log("Comment:" + commentStr);
			
			reader.BaseStream.Seek(283, SeekOrigin.Begin);
		}
		
		private void ReadMeshVertexData(BinaryReader reader)
		{
			// Read VertexCount
			vertexCount = reader.ReadUInt32();
			Debug.Log("VertexCount:" + vertexCount);
			
			// Create Vertex Arrays
			vertexPositions = new Vector3[vertexCount];
			vertexNormals = new Vector3[vertexCount];
			vertexUVs = new Vector2[vertexCount];
			boneWeights = new BoneWeight[vertexCount];
			
			
			for(int i = 0; i < vertexCount; i++)
			{
				// Read Position
				vertexPositions[i].x = reader.ReadSingle();
				vertexPositions[i].y = reader.ReadSingle();
				vertexPositions[i].z = reader.ReadSingle();
				
				// Read Normal
				vertexNormals[i].x = reader.ReadSingle();
				vertexNormals[i].y = reader.ReadSingle();
				vertexNormals[i].z = reader.ReadSingle();
				
				// Read UV
				vertexUVs[i].x = reader.ReadSingle();
				vertexUVs[i].y = reader.ReadSingle();
				
				// Read Bone Index
				boneWeights[i].boneIndex0 = reader.ReadUInt16();
				boneWeights[i].boneIndex1 = reader.ReadUInt16();
				// Read Bone Weight
				char weight = reader.ReadChar();
				boneWeights[i].weight0 = (float)weight * 0.01f;
				boneWeights[i].weight1 = 1.00f - boneWeights[i].weight0;
				
				reader.BaseStream.Seek(1, SeekOrigin.Current);
			}
		}
		
		// Read Face Index Data
		private void ReadVertexIndexes(BinaryReader reader)
		{
			faceVertexCount = reader.ReadUInt32();
			Debug.Log("Face Vertex Count:" + faceVertexCount);
			
			faceIndex = new int[faceVertexCount];
			
			for(int i = 0; i < faceVertexCount; i++)
			{
				faceIndex[i] = reader.ReadUInt16();
				//if(i < 12)	Debug.Log(faceIndex[i]);
			}
		}
		
		// Read MaterialData
		private void ReadMaterialData(BinaryReader reader)
		{
			materialCount = reader.ReadUInt32();
			Debug.Log("MaterialCount:" + materialCount);
			
			// Create MaterialData Array
			materials = new MaterialData[materialCount];
			
			long startPos;
			
			for(int i = 0; i < materialCount; i++)
			{
				startPos = reader.BaseStream.Position;
				
				materials[i] = new MaterialData();
				
				MaterialData mat = materials[i];
				
				//materials[i].diffuseColor[0] = reader.ReadSingle();
				//materials[i].diffuseColor[1] = reader.ReadSingle();
				//materials[i].diffuseColor[2] = reader.ReadSingle();
				ReadPluralFloat(reader, mat.diffuseColor, 3);
				//Debug.Log("DiffuseColor:" + materials[i].diffuseColor[0] + " " + materials[i].diffuseColor[1] + " " + materials[i].diffuseColor[2]);
				
				mat.alpha = reader.ReadSingle();
				
				mat.specularity = reader.ReadSingle();
				
				ReadPluralFloat(reader, mat.specularColor, 3);
				//mat.specularColor[0] = reader.ReadSingle();
				//mat.specularColor[1] = reader.ReadSingle();
				//mat.specularColor[2] = reader.ReadSingle();
				
				ReadPluralFloat(reader, mat.mirrorColor, 3);
				//mat.mirrorColor[0] = reader.ReadSingle();
				//mat.mirrorColor[1] = reader.ReadSingle();
				//mat.mirrorColor[2] = reader.ReadSingle();
				
				mat.toonIndex = reader.ReadByte();
				
				mat.edgeFlag = reader.ReadByte();
				
				mat.faceVertCount = reader.ReadUInt32();
				
				//mat.texturePath = reader.ReadChars(20);
				for(int j = 0; j < 20; j++)
				{
					mat.texturePath[j] = reader.ReadChar();
				}
				
				//Debug.Log("Current:" + reader.BaseStream.Position);
				
				reader.BaseStream.Seek(startPos + 70, SeekOrigin.Begin);
			}
		}
		
		// Read Bone Data
		private void ReadBoneData(BinaryReader reader)
		{
			boneCount = reader.ReadUInt16();
			Debug.Log("Bone Count:" + boneCount);
			
			bones = new BoneData[boneCount];
			
			for(int i = 0; i < boneCount; i++)
			{
				bones[i] = new BoneData();
				BoneData bone = bones[i];
				
				long start = reader.BaseStream.Position;
				
				bone.boneName = reader.ReadChars(20);
				reader.BaseStream.Seek(start + 20, SeekOrigin.Begin);
				bone.boneName[19] = '\0';
				
				bone.parentBoneIndex = reader.ReadUInt16();
				
				bone.tailPosBoneIndex = reader.ReadUInt16();
				
				bone.boneType = reader.ReadByte();
				
				bone.ikParentBoneIndex = reader.ReadUInt16();
				
				ReadPluralFloat(reader, bone.boneHeadPos, 3);
				
				//Debug.Log(i + " ParentBoneIndex:" + bone.parentBoneIndex);
				//Debug.Log("Current:" + reader.BaseStream.Position);
			}
		}
		
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}
