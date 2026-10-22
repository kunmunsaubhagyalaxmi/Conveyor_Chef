using Watermelon.BusStop;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PUUndoBehavior : PUBehavior
    {
        private PUUndoSettings customSettings;
        private BaseCharacterBehavior activeElement;
        private TweenCase rotateTweenCase;

        public override void Initialise()
        {
            customSettings = (PUUndoSettings)settings;
        }

        public override bool Activate()
        {
            activeElement = DockBehavior.LastPickedObject;

            if (activeElement == null)
                return false;

            // a pipe refills the spot as soon as a piece leaves it: the pipe takes that piece back
            if (LevelController.GetElementType(activeElement.ElementPosition) != LevelElement.Type.Empty &&
                !SpawnerElementBehavior.TakeBackFrom(activeElement.ElementPosition, activeElement.LevelElement.ElementType))
                return false;

            DockBehavior.RemoveLastPicked();

            IsBusy = true;

            Vector3[] path = MapUtils.CalculatePath(activeElement.ElementPosition.X, activeElement.ElementPosition.Y, LevelController.LevelMap);

            Vector3[] revertedPath = new Vector3[path.Length + 1];
            revertedPath[revertedPath.Length - 1] = LevelController.GetPosition(activeElement.ElementPosition);

            for (int i = 0; i < path.Length; i++)
            {
                revertedPath[i] = path[path.Length - 1 - i];
            }

            activeElement.MoveTo(revertedPath, false, OnMoveEnded);

            return true;
        }

        private void OnMoveEnded()
        {
            IsBusy = false;

            activeElement.ResetSubmitState();

            LevelController.PlaceElementOnMap(activeElement, activeElement.ElementPosition);

            rotateTweenCase = new TweenCaseRotateTowards(activeElement.transform, Quaternion.identity, 900, 0.15f);
            rotateTweenCase.SetDuration(float.MaxValue);
            rotateTweenCase.StartTween();

            activeElement = null;
        }

        public override void ResetBehavior()
        {
            IsBusy = false;

            activeElement = null;

            rotateTweenCase.KillActive();
        }
    }
}
