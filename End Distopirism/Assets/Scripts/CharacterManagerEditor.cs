using System.Linq;
using UnityEditor;
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
            EditorGUILayout.LabelField("��ų ���", EditorStyles.boldLabel);

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
        string skillFileName = System.IO.Path.GetFileNameWithoutExtension(skillAssetPath);

        EditorGUILayout.BeginVertical("box");

        DisplaySkillIcon(skill.Sprite);
        EditorGUILayout.LabelField($"��ų: {skillName}");
        EditorGUILayout.LabelField($"���� ��ġ / ��ų��ȣ: {skillFolderName} / {skillFileName}");
        EditorGUILayout.LabelField($"���ݷ�: {skill.MinDmg}~{skill.MaxDmg}");
        EditorGUILayout.LabelField($"�ִ� ���ݷ�: {skill.MaxDmg}");
        EditorGUILayout.LabelField($"���δ� ���: {skill.DmgUp}");

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DisplaySkillIcon(Sprite sprite)
    {
        if (sprite != null)
        {
            Texture2D texture = sprite.texture;
            float aspectRatio = (float)texture.width / texture.height;
            float width = MaxIconSize * (aspectRatio > 1 ? 1 : aspectRatio);
            float height = MaxIconSize * (aspectRatio > 1 ? 1 / aspectRatio : 1);

            EditorGUILayout.LabelField("��ų ������");
            Rect rect = GUILayoutUtility.GetRect(width, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.LabelField("��ų ������ ����");
        }
    }
}
