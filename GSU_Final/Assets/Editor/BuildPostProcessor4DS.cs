#if UNITY_IOS

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;


public class BuildPostProcessor4DS
{



    [PostProcessBuildAttribute(1)]
	public static void OnPostProcessBuild(BuildTarget target, string path)
	{
		if (target == BuildTarget.iOS)
		{
			// Read.
			string projectPath = PBXProject.GetPBXProjectPath(path);
			PBXProject project = new PBXProject();
			project.ReadFromString(File.ReadAllText(projectPath));
			string targetName = PBXProject.GetUnityTargetName();
			string targetGUID = project.TargetGuidByName(targetName);

			AddFrameworks(project, targetGUID);

			// Write.
			File.WriteAllText(projectPath, project.WriteToString());
		}
	}

	static void AddFrameworks(PBXProject project, string targetGUID)
	{
		project.AddFrameworkToProject(targetGUID, "libz.tbd", false);
	}

}

#endif