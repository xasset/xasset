using UnityEditor;
using System.Text;
using com.regina.fUnityTools.Editor;

public class OdinBuildTagEditorUtil
{
    private static string ODIN_BUILD_TAG_CLASS_PATH = "Assets/xasset/Editor/Odin/OdinBuildTagEnum.cs";

    private static int MAX_TAG_COUNT = 60; //TODO 暂只支持枚举

    //根据tags 生成枚举
    public static void WriteOdinBuildTagClassFile(string[] tags)
    {
        AssetDatabase.Refresh();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[System.Flags]");
        sb.AppendLine("public enum TagEnum : ulong");
        sb.AppendLine("{");
        sb.Append(GetEnumStr(tags)); //动态枚举
        sb.AppendLine("}");
        EditorFileUtils.WriteAssetFile(sb.ToString(), ODIN_BUILD_TAG_CLASS_PATH);
        AssetDatabase.Refresh();
    }

    private static string GetEnumStr(string[] tags)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\t none = 0,");
        int count = tags.Length > MAX_TAG_COUNT ? MAX_TAG_COUNT : tags.Length;
        for (int i = 0; i < count; i++)
        {
            string tag = tags[i];
            sb.AppendLine($"\t {tag} = 1 << {i},");
        }

        return sb.ToString();
    }
}