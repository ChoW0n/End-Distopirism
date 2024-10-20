using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterProfile))]
public class CharacterInspectorEditor : Editor
{
    private const float MaxIconSize = 100f; // 최대 아이콘 크기

    public override void OnInspectorGUI()
    {
        CharacterProfile characterProfile = (CharacterProfile)target;

        // 기본 인스펙터를 그립니다.
        DrawDefaultInspector();

        if (characterProfile.GetPlayer.skills != null && characterProfile.GetPlayer.skills.Count > 0)
        {
            EditorGUILayout.LabelField("캐릭터 스킬 목록", EditorStyles.boldLabel);

            foreach (var skill in characterProfile.GetPlayer.skills)
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
        string skillFolderName = "";

        if (!string.IsNullOrEmpty(skillAssetPath))
        {
            string directory = System.IO.Path.GetDirectoryName(skillAssetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                string[] folderNames = directory.Split('/');
                if (folderNames.Length > 0)
                {
                    skillFolderName = folderNames[folderNames.Length - 1];
                }
            }
        }

        EditorGUILayout.BeginVertical("box");

        DisplaySkillIcon(skill.sprite, skill);
        EditorGUILayout.LabelField($"스킬: {skillName}");
        EditorGUILayout.LabelField($"폴더: {skillFolderName}");
        EditorGUILayout.LabelField($"공격력: {skill.minDmg}~{skill.maxDmg}");
        EditorGUILayout.LabelField($"코인당 상승: {skill.dmgUp}");

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

            // 아이콘을 표시할 사각형 영역을 계산합니다.
            Rect rect = GUILayoutUtility.GetRect(width, height, GUI.skin.box);
            // GUI.DrawTexture를 사용하여 투명 배경을 지원합니다.
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            EditorGUILayout.LabelField(skillFileName);
        }
    }
}