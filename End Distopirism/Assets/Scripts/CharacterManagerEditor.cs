using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor
{
    private const float MaxIconSize = 100f; // 최대 아이콘 크기

    public override void OnInspectorGUI()
    {
        CharacterManager characterManager = (CharacterManager)target;

        // 기본 인스펙터를 그립니다.
        DrawDefaultInspector();

        if (characterManager.skills != null && characterManager.skills.Count > 0)
        {
            EditorGUILayout.LabelField("스킬 목록", EditorStyles.boldLabel);

            foreach (var skill in characterManager.skills)
            {
                if (skill == null) continue;

                // 스킬의 정보와 아이콘 표시
                DisplaySkillInfo(skill);
            }
        }
        else
        {
            EditorGUILayout.LabelField("스킬 없음");
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
        EditorGUILayout.LabelField($"스킬: {skillName}");
        EditorGUILayout.LabelField($"폴더 위치 / 스킬번호: {skillFolderName} / {skillFileName}");
        EditorGUILayout.LabelField($"공격력: {skill.MinDmg}~{skill.MaxDmg}");
        EditorGUILayout.LabelField($"최대 공격력: {skill.MaxDmg}");
        EditorGUILayout.LabelField($"코인당 상승: {skill.DmgUp}");

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

            EditorGUILayout.LabelField("스킬 아이콘");
            Rect rect = GUILayoutUtility.GetRect(width, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.LabelField("스킬 아이콘 없음");
        }
    }
}
