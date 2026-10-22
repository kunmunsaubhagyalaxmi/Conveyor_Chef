using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon.BusStop
{
    public class SpawnerElementBehavior : LevelElementBehavior
    {
        [SerializeField] Transform graphicsTransform;

        [SerializeField] TextMeshPro amountText;

        [SerializeField] JuicyBounce juicyBounce;

        private List<LevelElement.Type> spawnQueue;
        private int currentElementIndex;
        private ElementPosition spawnerOffset;

        private TweenCase delayTweenCase;

        // The last piece out of the pipe while it still stands in front of it. The number on the
        // pipe counts it too, so it reads 1 (not 0) while the last piece is still waiting there.
        private LevelElementBehavior waiting;

        public override void Initialise(LevelElement levelElement, ElementPosition elementPosition)
        {
            base.Initialise(levelElement, elementPosition);

            juicyBounce.Initialise(transform);
        }

        public override void SetExtraData(object extraData)
        {
            SpawnerData spawnerData = (SpawnerData)extraData;

            graphicsTransform.rotation = Quaternion.Euler(0, 90 + 90 * (int)spawnerData.Direction, 0);

            spawnerOffset = elementPosition + ElementPosition.GetOffset(spawnerData.Direction);

            spawnQueue = new List<LevelElement.Type>(spawnerData.SpawnQueue);
            currentElementIndex = spawnQueue.Count - 1;

            waiting = null;
            ShowCount();
        }

        // pieces still inside the pipe, plus the one waiting in front of it
        private void ShowCount()
        {
            amountText.text = (currentElementIndex + 1 + (waiting != null ? 1 : 0)).ToString();
        }

        public override void OnMapUpdated()
        {
            // the waiting piece was picked (or taken by a hint): it no longer counts
            if (waiting != null && LevelController.GetElementType(spawnerOffset) == LevelElement.Type.Empty)
            {
                waiting = null;
                ShowCount();
            }

            delayTweenCase = Tween.DelayedCall(0.1f, () =>
            {
                TryToSpawnElement();
            });
        }

        private void TryToSpawnElement()
        {
            if (currentElementIndex < 0)
                return;

            if (LevelController.GetElementType(spawnerOffset) != LevelElement.Type.Empty)
                return;

            LevelElement.Type nextElementType = spawnQueue[currentElementIndex];

            LevelElementBehavior spawnedElement = LevelController.SpawnLevelObject(nextElementType, spawnerOffset, false, true);
            spawnedElement.PlaySpawnAnimation();

            currentElementIndex--;
            waiting = spawnedElement;
            ShowCount();

            juicyBounce.Bounce();
        }

        public bool TryToRemoveElement(LevelElement.Type type)
        {
            if(currentElementIndex >= 0)
            {
                for(int i = 0; i <= currentElementIndex; i++)
                {
                    if(spawnQueue[i] == type)
                    {
                        spawnQueue.RemoveAt(i);

                        currentElementIndex--;
                        ShowCount();

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// UNDO of a piece that stood in front of a pipe: the pipe has already pushed its next
        /// piece into that spot, so it takes that piece back inside (the count goes up again) and
        /// the spot is kept for the piece coming back. False when no pipe's waiting piece is there.
        /// </summary>
        public static bool TakeBackFrom(ElementPosition position, LevelElement.Type returning)
        {
            foreach (LevelElementBehavior element in LevelController.GetLevelElementsByType(LevelElement.Type.Spawner))
            {
                if (element is SpawnerElementBehavior spawner && spawner.TakeBack(position, returning))
                    return true;
            }
            return false;
        }

        private bool TakeBack(ElementPosition position, LevelElement.Type returning)
        {
            if (waiting == null || !spawnerOffset.Equals(position) || !waiting.ElementPosition.Equals(position))
                return false;
            int back = currentElementIndex + 1;     // the waiting piece's place in the queue
            if (back >= spawnQueue.Count || spawnQueue[back] != waiting.LevelElement.ElementType)
                return false;

            delayTweenCase.KillActive();
            LevelController.TakeOffBoard(waiting, returning);
            waiting = null;
            currentElementIndex = back;
            ShowCount();
            juicyBounce.Bounce();
            return true;
        }

        public override void Unload()
        {
            delayTweenCase.KillActive();
            waiting = null;
        }

        public override bool IsPlayableElement()
        {
            return currentElementIndex >= 0;          // index 0 is the last piece inside, still to come
        }
    }
}
