using MessengerRando.Archipelago;

namespace MessengerRando.Lifecycle;

public interface ISaveLifecycleHandler
{
    void OnLoad(ArchipelagoData save);
    void OnSave(ArchipelagoData save);
}
