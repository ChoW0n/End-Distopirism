using System.Collections;
using UnityEngine;

public class BattleMoveManager
{
    private float moveSpeed;
    private float battleSpacing;
    private float winnerMoveDistance;
    private float loserMoveDistance;

    public BattleMoveManager(float moveSpeed, float battleSpacing, float winnerMoveDistance, float loserMoveDistance)
    {
        this.moveSpeed = moveSpeed;
        this.battleSpacing = battleSpacing;
        this.winnerMoveDistance = winnerMoveDistance;
        this.loserMoveDistance = loserMoveDistance;
    }

    public IEnumerator MoveToBattlePosition(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Vector3 midpoint = (playerObject.transform.position + targetObject.transform.position) / 2;
        Vector3 playerDirection = (midpoint - playerObject.transform.position).normalized;
        Vector3 targetDirection = (midpoint - targetObject.transform.position).normalized;

        Vector3 playerDestination = midpoint - playerDirection * (battleSpacing / 2);
        Vector3 targetDestination = midpoint - targetDirection * (battleSpacing / 2);

        yield return MoveCharacter(playerObject, playerDestination);
        yield return MoveCharacter(targetObject, targetDestination);
    }

    public IEnumerator MoveBattleResult(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Vector3 midpoint = (playerObject.transform.position + targetObject.transform.position) / 2;
        Vector3 playerDirection = (midpoint - playerObject.transform.position).normalized;
        Vector3 targetDirection = (midpoint - targetObject.transform.position).normalized;

        Vector3 playerDestination, targetDestination;
        CharacterProfile loser, winner;

        if (playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg)
        {
            winner = playerObject;
            loser = targetObject;
            playerDestination = playerObject.transform.position + playerDirection * winnerMoveDistance;
            targetDestination = targetObject.transform.position - targetDirection * loserMoveDistance;
        }
        else if (playerObject.GetPlayer.dmg < targetObject.GetPlayer.dmg)
        {
            winner = targetObject;
            loser = playerObject;
            playerDestination = playerObject.transform.position - playerDirection * loserMoveDistance;
            targetDestination = targetObject.transform.position + targetDirection * winnerMoveDistance;
        }
        else
        {
            yield break;
        }

        yield return MoveCharacter(loser, loser == playerObject ? playerDestination : targetDestination);
        yield return MoveCharacter(winner, winner == playerObject ? playerDestination : targetDestination);
    }

    public IEnumerator MoveBack(CharacterProfile character, Vector3 originalPosition)
    {
        yield return MoveCharacter(character, originalPosition);
    }

    private IEnumerator MoveCharacter(CharacterProfile character, Vector3 destination)
    {
        while (character.transform.position != destination)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
