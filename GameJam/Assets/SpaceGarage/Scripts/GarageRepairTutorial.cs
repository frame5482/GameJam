using UnityEngine;

namespace SpaceGarage
{
    // Bridges real workshop outcomes to the existing TutorialManager.
    public sealed class GarageRepairTutorial : MonoBehaviour
    {
        public const string BeginKey = "Garage.FirstRepair.Begin";
        public const string DockedKey = "Garage.FirstRepair.Docked";
        public const string RepairedKey = "Garage.FirstRepair.OnePartRepaired";
        [SerializeField] TutorialManager tutorial;
        [SerializeField] CustomerRepairManager workshop;
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] GameObject startPanel;
        bool began, docked, repaired;

        void OnEnable()
        {
            if (workshop != null) workshop.Changed += PublishProgress;
            if (tutorial != null) tutorial.onStarted.AddListener(ResetProgress);
        }

        void Update()
        {
            if (began || tutorial == null || TutorialManager.IsDone(tutorial.SaveKey)) return;
            if (mainMenuPanel == null || !mainMenuPanel.activeInHierarchy ||
                (startPanel != null && startPanel.activeInHierarchy)) return;
            began = true;
            TutorialSignals.Raise(BeginKey);
        }

        void ResetProgress()
        {
            docked = repaired = false;
            TutorialSignals.Consume(DockedKey);
            TutorialSignals.Consume(RepairedKey);
            PublishProgress();
        }

        void PublishProgress()
        {
            if (tutorial == null || !tutorial.IsRunning || workshop == null) return;
            if (!docked && workshop.State == CustomerRepairState.Docked)
            { docked = true; TutorialSignals.Raise(DockedKey); }
            if (!repaired && workshop.State == CustomerRepairState.Docked && workshop.Customer != null)
                foreach (var part in workshop.Customer.Parts)
                    if (part != null && part.OriginalDamagePercent > 0 && part.IsRepaired)
                    { repaired = true; TutorialSignals.Raise(RepairedKey); break; }
        }

        void OnDisable()
        {
            if (workshop != null) workshop.Changed -= PublishProgress;
            if (tutorial != null) tutorial.onStarted.RemoveListener(ResetProgress);
        }
    }
}
