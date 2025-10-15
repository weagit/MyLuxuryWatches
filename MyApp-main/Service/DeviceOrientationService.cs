using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Service
{
    public partial class DeviceOrientationService
    {
        // Buffer de réception qui notifie les changements
        public QueueBuffer SerialBuffer = new();

        // Déclaration des méthodes partielles pour ouvrir et fermer le port.
        // Leur implémentation se fera dans le fichier spécifique à Windows.
        public partial void OpenPort();
        public partial void ClosePort();

        // Classe interne qui hérite de Queue et déclenche un événement à chaque ajout
        public sealed partial class QueueBuffer : Queue
        {
            public event EventHandler? Changed;
            public override void Enqueue(object? obj)
            {
                base.Enqueue(obj);
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
