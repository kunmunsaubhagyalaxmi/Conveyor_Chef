using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// EXTRA PLATE: one more spot on the tray for the rest of the level. Once per level: the
    /// button stays busy (greyed) until the level ends, when the tray goes back to its spots.
    /// </summary>
    public class PUExtraSlotBehavior : PUBehavior
    {
        public override void Initialise()
        {

        }

        public override bool Activate()
        {
            if (DockBehavior.HasExtraSlot || !DockBehavior.AddExtraSlot())
                return false;

            IsBusy = true;
            return true;
        }

        public override string GetFloatingMessage()
        {
            return Loc.T(settings.FloatingMessage);
        }

        public override void ResetBehavior()
        {
            IsBusy = false;
        }
    }
}
