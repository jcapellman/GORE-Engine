#include "GoreEngine.h"
#include "GoreMain.h"

#ifdef WIN32
int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
#else
int main() {
#endif
    GoreMain gMain = GoreMain("Shoot a Rama 2025");

    gMain.Init();

    gMain.Run();

    gMain.Shutdown();
}
