﻿//#define VERBOSE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Mogre;

namespace Ponykart.Stuff {
	/// <summary>
	/// Class for loading a .scene file! :D
	/// Not made by me. This is pretty much a copy and paste job from something someone made on the ogre wiki.
	/// .scene is kinda a de facto standardised format so there are a bunch of tools that export stuff to it.
	/// 
	/// This needs to be in the kernel because multiple things use it during loading!
	/// </summary>
	public class DotSceneLoader {

		public List<string> DynamicObjects;
		public List<string> StaticObjects;

		protected SceneNode attachNode;
		protected SceneManager sceneMgr;
		protected String groupName;
		protected String prependNode;
		protected string sceneFileName;

		public void ParseDotScene(String SceneName, String groupName) {
			ParseDotScene(SceneName, groupName, null, string.Empty);
		}

		public void ParseDotScene(String SceneName, String groupName, SceneNode pAttachNode) {
			ParseDotScene(SceneName, groupName, pAttachNode, string.Empty);
		}

		public void ParseDotScene(String SceneName, String groupName1, SceneNode pAttachNode, String sPrependNode) {
			// set up shared object values
			this.groupName = groupName1;
			this.sceneMgr = LKernel.GetG<SceneManager>();
			this.prependNode = sPrependNode;
			this.sceneFileName = SceneName;
			this.StaticObjects = new List<string>();
			this.DynamicObjects = new List<string>();

			XmlDocument XMLDoc = null;
			XmlElement XMLRoot;

			DataStreamPtr pStream = ResourceGroupManager.Singleton.OpenResource(SceneName, groupName);

			String data = pStream.AsString;
			// Open the .scene File
			XMLDoc = new XmlDocument();
			XMLDoc.LoadXml(data);
			pStream.Close();

			// Validate the File
			XMLRoot = XMLDoc.DocumentElement;
			if (XMLRoot.Name != "scene") {
				Launch.Log("[DotSceneLoader] Error: Invalid .scene File. Missing <scene>. [File: " + sceneFileName + "]");
				return;
			}

			// figure out where to attach any nodes we create
			attachNode = pAttachNode;
			if (attachNode == null)
				attachNode = sceneMgr.RootSceneNode;

			// Process the scene
			processScene(XMLRoot);
		}

		protected float ParseFloat(String s) {
			NumberFormatInfo provider = new NumberFormatInfo();
			provider.NumberDecimalSeparator = ".";
			return float.Parse(s, provider);
		}

		protected String getAttrib(XmlElement XMLNode, String attrib) {
			return getAttrib(XMLNode, attrib, "");
		}

		protected String getAttrib(XmlElement XMLNode, String attrib, String defaultValue) {
			if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return XMLNode.GetAttribute(attrib);
			else
				return defaultValue;
		}

		protected bool getAttribBool(XmlElement XMLNode, String parameter) {
			return getAttribBool(XMLNode, parameter, false);
		}

		protected bool getAttribBool(XmlElement XMLNode, String attrib, bool defaultValue) {
			if (string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return defaultValue;

			if (XMLNode.GetAttribute(attrib) == "true")
				return true;

			return false;
		}

		protected float getAttribReal(XmlElement XMLNode, String parameter) {
			return getAttribReal(XMLNode, parameter, 0.0f);
		}

		protected float getAttribReal(XmlElement XMLNode, String attrib, float defaultValue) {
			if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
				return ParseFloat(XMLNode.GetAttribute(attrib));
			else
				return defaultValue;
		}

		protected ColourValue parseColour(XmlElement XMLNode) {
			return new ColourValue(
			   ParseFloat(XMLNode.GetAttribute("r")),
			   ParseFloat(XMLNode.GetAttribute("g")),
			   ParseFloat(XMLNode.GetAttribute("b")),
			   string.IsNullOrEmpty(XMLNode.GetAttribute("a")) == false ? ParseFloat(XMLNode.GetAttribute("a")) : 1
			  );
		}

		protected Quaternion parseQuaternion(XmlElement XMLNode) {
			Quaternion orientation = new Quaternion();

			orientation.x = ParseFloat(XMLNode.GetAttribute("x"));
			orientation.y = ParseFloat(XMLNode.GetAttribute("y"));
			orientation.z = ParseFloat(XMLNode.GetAttribute("z"));
			orientation.w = ParseFloat(XMLNode.GetAttribute("w"));

			return orientation;
		}

		protected Quaternion parseRotation(XmlElement XMLNode) {
			Quaternion orientation = new Quaternion();

			orientation.x = ParseFloat(XMLNode.GetAttribute("qx"));
			orientation.y = ParseFloat(XMLNode.GetAttribute("qy"));
			orientation.z = ParseFloat(XMLNode.GetAttribute("qz"));
			orientation.w = ParseFloat(XMLNode.GetAttribute("qw"));

			return orientation;
		}

		protected Vector3 parseVector3(XmlElement XMLNode) {
			return new Vector3(
			   ParseFloat(XMLNode.GetAttribute("x")),
			   ParseFloat(XMLNode.GetAttribute("y")),
			   ParseFloat(XMLNode.GetAttribute("z"))
			  );
		}

		protected void processCamera(XmlElement XMLNode, SceneNode pParent) {

			// Process attributes
			String name = getAttrib(XMLNode, "name");

			// Create the light
			Camera pCamera = sceneMgr.CreateCamera(name);
			if (pParent != null)
				pParent.AttachObject(pCamera);

			float pFov = getAttribReal(XMLNode, "fov", 45);
			pCamera.FOVy = new Degree(pFov);

			String sValue = getAttrib(XMLNode, "projectionType", "perspective");
			if (sValue == "perspective")
				pCamera.ProjectionType = ProjectionType.PT_PERSPECTIVE;
			else if (sValue == "orthographic")
				pCamera.ProjectionType = ProjectionType.PT_ORTHOGRAPHIC;

			XmlElement pElement;

			// Process normal (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("clipping");
			if (pElement != null) {
				// Blender
				float nearDist = getAttribReal(pElement, "nearPlaneDist");
				if (nearDist == 0) {
					// 3ds
					nearDist = getAttribReal(pElement, "near");
				}
				pCamera.NearClipDistance = nearDist;

				// Blender
				float farDist = getAttribReal(pElement, "farPlaneDist");
				if (farDist == 0) {
					// 3ds
					farDist = getAttribReal(pElement, "far");
				}
				pCamera.FarClipDistance = farDist;
			}
#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed camera \"" + name + "\"");
#endif
		}

		protected void processEntity(XmlElement XMLNode, SceneNode pParent) {
			// Process attributes
			String name = getAttrib(XMLNode, "name");
			String meshFile = getAttrib(XMLNode, "meshFile");

			bool bstatic = getAttribBool(XMLNode, "static", false);
			if (bstatic)
				StaticObjects.Add(name);
			else
				DynamicObjects.Add(name);

			bool bvisible = getAttribBool(XMLNode, "visible", true);
			bool bcastshadows = getAttribBool(XMLNode, "castShadows", true);
			float brenderingDistance = getAttribReal(XMLNode, "renderingDistance", 0);

			// Create the entity
			Entity pEntity = null;
			try {
				MeshPtr mesh = MeshManager.Singleton.Load(meshFile, groupName);
				ushort src, dest;
				mesh.SuggestTangentVectorBuildParams(VertexElementSemantic.VES_TANGENT, out src, out dest);
				mesh.BuildTangentVectors(VertexElementSemantic.VES_TANGENT, src, dest);

				pEntity = sceneMgr.CreateEntity(name, meshFile);
				pEntity.Visible = bvisible;
				pEntity.CastShadows = bcastshadows;
				pEntity.RenderingDistance = brenderingDistance;

				XmlElement pElement;
				// Process subentities (?)
				pElement = (XmlElement) XMLNode.SelectSingleNode("subentities");
				if (pElement != null) {
					pElement = (XmlElement) pElement.FirstChild;
					while (pElement != null) {
						string mat = getAttrib(pElement, "materialName");
						pEntity.SetMaterialName(mat);
						pElement = (XmlElement) pElement.NextSibling;
					}
				}

				pParent.AttachObject(pEntity);
			}
			catch (Exception e) {
				Launch.Log("[DotSceneLoader] Error loading an entity!" + e.Message + " [File: " + sceneFileName + "]");
			}

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed entity \"" + name + "\"");
#endif
		}

		protected void processEnvironment(XmlElement XMLNode) {
			XmlElement pElement;

			// Process fog (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("fog");
			if (pElement != null)
				processFog(pElement);

			// Process colourAmbient (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("colourAmbient");
			if (pElement != null)
				sceneMgr.AmbientLight = parseColour(pElement);

			// Process colourBackground (?)
			//! @todo Set the background colour of all viewports (RenderWindow has to be provided then)
			//            pElement = (XmlElement)XMLNode.SelectSingleNode("colourBackground");
			//            if (pElement != null)
			//                ;//mSceneMgr->set(parseColour(pElement));

			//            // Process userDataReference (?)
			//            pElement = (XmlElement)XMLNode.SelectSingleNode("userData");
			//            if (pElement != null)
			//                processUserDataReference(pElement);
#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed environment");
#endif
		}

		protected void processFog(XmlElement XMLNode) {
			// Process attributes
			float linearStart = getAttribReal(XMLNode, "linearStart", 0.0f);
			float linearEnd = getAttribReal(XMLNode, "linearEnd", 1.0f);

			FogMode mode = FogMode.FOG_NONE;
			String sMode = getAttrib(XMLNode, "mode");
			// only linear atm
			if (sMode == "none")
				mode = FogMode.FOG_NONE;
			else if (sMode == "exp")
				mode = FogMode.FOG_EXP;
			else if (sMode == "exp2")
				mode = FogMode.FOG_EXP2;
			else if (sMode == "linear")
				mode = FogMode.FOG_LINEAR;

			XmlElement pElement;

			// Process colourDiffuse (?)
			ColourValue colourDiffuse = ColourValue.White;
			pElement = (XmlElement) XMLNode.SelectSingleNode("colourDiffuse");
			if (pElement != null)
				colourDiffuse = parseColour(pElement);

			// Setup the fog
			sceneMgr.SetFog(mode, colourDiffuse, 0.001f, linearStart, linearEnd);
#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed fog");
#endif
		}

		protected void processLight(XmlElement XMLNode, SceneNode pParent) {
			// Process attributes
			String name = getAttrib(XMLNode, "name");

			// Create the light
			Light pLight = sceneMgr.CreateLight(name);
			if (pParent != null)
				pParent.AttachObject(pLight);

			String sValue = getAttrib(XMLNode, "type");
			if (sValue == "point")
				pLight.Type = Light.LightTypes.LT_POINT;
			else if (sValue == "directional")
				pLight.Type = Light.LightTypes.LT_DIRECTIONAL;
			else if (sValue == "spotLight")
				pLight.Type = Light.LightTypes.LT_SPOTLIGHT;

			// only set if Lamp is Spotlight (Blender)
			bool castShadow = true;
			if (XMLNode.HasAttribute("castShadow")) {
				castShadow = getAttribBool(XMLNode, "castShadow", true);
			}
			else if (XMLNode.HasAttribute("castShadows")) {
				castShadow = getAttribBool(XMLNode, "castShadows", true);
			}

			pLight.CastShadows = castShadow;

			XmlElement pElement;

			// Process normal (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("normal");
			if (pElement != null)
				pLight.Direction = parseVector3(pElement);

			// Process colourDiffuse (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("colourDiffuse");
			if (pElement != null)
				pLight.DiffuseColour = parseColour(pElement);

			// Process colourSpecular (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("colourSpecular");
			if (pElement != null)
				pLight.SpecularColour = parseColour(pElement);

			// Process lightRange (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("lightRange");
			if (pElement != null)
				processLightRange(pElement, pLight);

			// Process lightAttenuation (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("lightAttenuation");
			if (pElement != null)
				processLightAttenuation(pElement, pLight);

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed light \"" + name + "\"");
#endif
		}

		protected void processLightAttenuation(XmlElement XMLNode, Light pLight) {
			// Process attributes
			float range = getAttribReal(XMLNode, "range");
			float constant = getAttribReal(XMLNode, "constant");
			float linear = getAttribReal(XMLNode, "linear");
			float quadratic = getAttribReal(XMLNode, "quadratic");

			// Setup the light attenuation
			pLight.SetAttenuation(range, constant, linear, quadratic);

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed light attenuation");
#endif
		}

		protected void processLightRange(XmlElement XMLNode, Light pLight) {
			// Process attributes
			float inner = getAttribReal(XMLNode, "inner");
			float outer = getAttribReal(XMLNode, "outer");
			float falloff = getAttribReal(XMLNode, "falloff", 1.0f);

			// Setup the light range
			pLight.SetSpotlightRange(new Radian((Degree) inner), new Radian((Degree) outer), falloff);

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed light range");
#endif
		}

		protected void processNode(XmlElement XMLNode, SceneNode pParent) {
			// Construct the node's name
			String name = prependNode + getAttrib(XMLNode, "name");

			// Create the scene node
			SceneNode pNode;
			if (name.Length == 0) {
				// Let Ogre choose the name
				if (pParent != null)
					pNode = pParent.CreateChildSceneNode();
				else
					pNode = attachNode.CreateChildSceneNode();
			}
			else {
				// Provide the name
				if (pParent != null)
					pNode = pParent.CreateChildSceneNode(name);
				else
					pNode = attachNode.CreateChildSceneNode(name);
			}

			// Process other attributes
			XmlElement pElement;

			// Process position (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("position");
			if (pElement != null) {
				pNode.Position = parseVector3(pElement);
				pNode.SetInitialState();
			}

			// Process quaternion (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("quaternion");
			if (pElement != null) {
				pNode.Orientation = parseQuaternion(pElement);
				pNode.SetInitialState();
			}

			// Process rotation (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("rotation");
			if (pElement != null) {
				pNode.Orientation = parseRotation(pElement);
				pNode.SetInitialState();
			}

			// Process scale (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("scale");
			if (pElement != null) {
				pNode.SetScale(parseVector3(pElement));
				pNode.SetInitialState();
			}

			// Process entity (*)
			pElement = (XmlElement) XMLNode.SelectSingleNode("entity");
			if (pElement != null) {
				processEntity(pElement, pNode);
			}

			// Process light (*)
			pElement = (XmlElement) XMLNode.SelectSingleNode("light");
			if (pElement != null) {
				processLight(pElement, pNode);
			}

			// Process plane (*)
			pElement = (XmlElement) XMLNode.SelectSingleNode("plane");
			while (pElement != null) {
				processPlane(pElement, pNode);
				pElement = (XmlElement) pElement.NextSibling;
			}



			// Process camera (*)
			pElement = (XmlElement) XMLNode.SelectSingleNode("camera");
			if (pElement != null) {
				processCamera(pElement, pNode);
			}

			// Process userDataReference (?)
			pElement = (XmlElement) XMLNode.SelectSingleNode("userData");
			if (pElement != null)
				processUserDataReference(pElement, pNode);

			// Process childnodes
			pElement = (XmlElement) XMLNode.SelectSingleNode("node");
			while (pElement != null) {
				processNode(pElement, pNode);
				pElement = (XmlElement) pElement.NextSibling;
			}

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed scene node \"" + name + "\"");
#endif
		}

		protected void processPlane(XmlElement XMLNode, SceneNode pParent) {

			string name = getAttrib(XMLNode, "name");
			float distance = getAttribReal(XMLNode, "distance");
			float width = getAttribReal(XMLNode, "width");
			float height = getAttribReal(XMLNode, "height");

			int xSegments = (int) getAttribReal(XMLNode, "xSegments");
			int ySegments = (int) getAttribReal(XMLNode, "ySegments");
			ushort numTexCoordSets = (ushort) getAttribReal(XMLNode, "numTexCoordSets");
			float uTile = getAttribReal(XMLNode, "uTile");
			float vTile = getAttribReal(XMLNode, "vTile");
			string material = getAttrib(XMLNode, "material");
			bool normals = getAttribBool(XMLNode, "normals");
			bool movablePlane = getAttribBool(XMLNode, "movablePlane");
			bool castShadows = getAttribBool(XMLNode, "castShadows");
			bool receiveShadows = getAttribBool(XMLNode, "receiveShadows");

			Vector3 normal = Vector3.ZERO;
			XmlElement pElement = (XmlElement) XMLNode.SelectSingleNode("normal");
			if (pElement != null)
				normal = parseVector3(pElement);

			Vector3 upVector = Vector3.UNIT_Y;
			pElement = (XmlElement) XMLNode.SelectSingleNode("upVector");
			if (pElement != null)
				upVector = parseVector3(pElement);

			Plane pPlane = new Plane(normal, upVector);

			Entity pEntity = null;
			try {
				MeshPtr ptr = MeshManager.Singleton.CreatePlane(name, groupName, pPlane, width, height, xSegments, ySegments, normals, numTexCoordSets, uTile, vTile, upVector);
				pEntity = sceneMgr.CreateEntity(name, name);
				pParent.AttachObject(pEntity);
			}
			catch (Exception e) {
				Launch.Log("[DotSceneLoader] Error loading an entity!" + e.Message + " [File: " + sceneFileName + "]");
			}

#if VERBOSE
			Launch.Log("[DotSceneLoader] Successfully processed plane \"" + name + "\"");
#endif
		}

		protected void processNodes(XmlElement XMLNode) {
			XmlElement pElement;

			// Process node (*)
			pElement = (XmlElement) XMLNode.SelectSingleNode("node");
			while (pElement != null) {
				processNode(pElement, null);
				XmlNode nextNode = pElement.NextSibling;
				pElement = nextNode as XmlElement;
				while (pElement == null && nextNode != null) {
					nextNode = nextNode.NextSibling;
					pElement = nextNode as XmlElement;
				}
			}
		}

		protected void processScene(XmlElement XMLRoot) {
			// Process the scene parameters
			String version = getAttrib(XMLRoot, "formatVersion", "unknown");

			Launch.Log("[DotSceneLoader] ==== Parsing dotScene file with version " + version + " [File: " + sceneFileName + "] ====");

			XmlElement pElement;

			// Process nodes (?)
			pElement = (XmlElement) XMLRoot.SelectSingleNode("nodes");
			if (pElement != null)
				processNodes(pElement);

			// Process environment (?)
			pElement = (XmlElement) XMLRoot.SelectSingleNode("environment");
			if (pElement != null)
				processEnvironment(pElement);
			// Process externals (?)
			//         pElement = (XmlElement)XMLRoot.SelectSingleNode("externals");
			//         if (pElement != null)
			//            processExternals(pElement);

			Launch.Log("[DotSceneLoader] ==== Successfully parsed dotScene file with version " + version + " [File: " + sceneFileName + "] ====");
		}

		protected void processUserDataReference(XmlElement XMLNode, SceneNode pNode) {
			// if you have any other attributes in your .scene files, you can put code to interpret them here!
		}
	}
}
