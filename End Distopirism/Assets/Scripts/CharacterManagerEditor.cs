using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor
{
    private const float MaxIconSize = 100f; // �ִ� ������ ũ��

    public override void OnInspectorGUI()
    {
        CharacterManager characterManager = (CharacterManager)target;

        // �⺻ �ν����͸� �׸��ϴ�.
        DrawDefaultInspector();

        if (characterManager.skills != null && characterManager.skills.Count > 0)
        {
            EditorGUILayout.LabelField("ĳ���� ��ų ���", EditorStyles.boldLabel);

            foreach (var skill in characterManager.skills)
            {
                if (skill == null) continue;

                // ��ų�� ������ ������ ǥ��
                DisplaySkillInfo(skill);
            }
        }
        else
        {
            EditorGUILayout.LabelField("��ų ����");
        }
    }

    private void DisplaySkillInfo(Skill skill)
    {
        string skillName = skill.skillName;
        string skillAssetPath = AssetDatabase.GetAssetPath(skill);
        string skillFolderName = System.IO.Path.GetDirectoryName(skillAssetPath).Split('/').Last();
        

        EditorGUILayout.BeginVertical("box");

        DisplaySkillIcon(skill.Sprite, skill);
        EditorGUILayout.LabelField($"��ų: {skillName}");
        EditorGUILayout.LabelField($"����: {skillFolderName}");
        EditorGUILayout.LabelField($"���ݷ�: {skill.MinDmg}~{skill.MaxDmg}");
        EditorGUILayout.LabelField($"���δ� ���: {skill.DmgUp}");

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DisplaySkillIcon(Sprite sprite, Skill skill)
    {
        string skillAssetPath = AssetDatabase.GetAssetPath(skill);
        string skillFileName = System.IO.Path.GetFileNameWithoutExtension(skillAssetPath);
        if (sprite != null)
        {
            Texture2D texture = sprite.texture;
            float aspectRatio = (float)texture.width / texture.height;
            float width = MaxIconSize * (aspectRatio > 1 ? 1 : aspectRatio);
            float height = MaxIconSize * (aspectRatio > 1 ? 1 / aspectRatio : 1);

            EditorGUILayout.LabelField(skillFileName);

            // �������� ǥ���� �簢�� ������ ����մϴ�.
            Rect rect = GUILayoutUtility.GetRect(width, height, GUI.skin.box);
            // GUI.DrawTexture�� ����Ͽ� ���� ����� �����մϴ�.
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            EditorGUILayout.LabelField(skillFileName);
        }
    }
}
