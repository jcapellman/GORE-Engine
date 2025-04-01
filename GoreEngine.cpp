#include "GoreEngine.h"
#include "GoreMain.h"

int main()
{
    GoreMain gMain = GoreMain("Shoot a Rama 2025");

    gMain.Init();

    gMain.Run();

    gMain.Shutdown();
}
