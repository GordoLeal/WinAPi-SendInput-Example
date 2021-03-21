using System;
using System.Threading;
using System.Runtime.InteropServices;

public class SendInputExample
{
    //Estrutura de input
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public short wVK;
        public short wScan;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct Input
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(8)]
        public MouseInput mi;
        [FieldOffset(8)]
        public KeyboardInput ki;
        [FieldOffset(8)]
        public HardwareInput hi;
    };

    [DllImport("USER32.DLL", SetLastError = true)] //Função responsavel por mandar os inputs
    public static extern int SendInput(short cInputs, Input[] inputs, int cbSize); //inputs amount, inputs array, inputs in byte size. Marshal.SizeOf(typeof(Inputs))

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] //pegar um caractere Virtual (https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) e transformar em ScanKey
    static extern short VkKeyScanExA(char ch, IntPtr dwhkl);

    [DllImport("user32.dll")] //adaptar o ScanKey em char aceitavel pelo windows hospedeiro do programa
    static extern short MapVirtualKeyExA(int uCode, int uMapType, IntPtr dwhkl);

    [DllImport("user32.dll")] //só por ter mesmo, não sei o que faz. O documento pede para ter.
    static extern IntPtr GetMessageExtraInfo();
    [DllImport("user32.dll")] //necessario pegar o layout do primeiro teclado que estiver na maquina.
    static extern IntPtr GetKeyboardLayout(ushort idThread);

    const int INPUTMOUSE = 0;
    const int INPUT_KEYBOARD = 1;

    const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    const int KEYEVENTF_KEYUP = 0x0002;
    const int KEYEVENTF_UNICODE = 0X0004;
    const int KEYEVENTF_SCANCODE = 0x0008;

    const int MAPVK_VK_TO_VSC = 0;
    
    //======= EXEMPLO DE INPUT DE TECLADO ==========
    //Exemplo de função inserindo uma entrada de botão W e saida de W um segundo depois.
    //SCANCODE foi utilizado pois esse codigo iria ser utilizado com aplicações DirectX.
    //SCANCODE simula inserção de comandos na fila de input do windows.
    public void sendTheMessage() //nome temporario, chamar na classe main
    {
        Input[] sInputs;
        sInputs = new Input[1];
        Thread.Sleep(4000); // para dar tempo para abrir manualmente a janela que as ações irão ser executadas, o ideal é abrir via codigo

        sInputs[0].type = INPUT_KEYBOARD;
        sInputs[0].ki.wVK = 0; //ao usar SCANCODE, wVK deve ser 0
        sInputs[0].ki.wScan = MapVirtualKeyExA(VkKeyScanExA('w', GetKeyboardLayout(0)), MAPVK_VK_TO_VSC, GetKeyboardLayout(0));
        sInputs[0].ki.dwExtraInfo = GetMessageExtraInfo(); //ainda não sei para que isso serve, mas ta ai.
        sInputs[0].ki.dwFlags = KEYEVENTF_SCANCODE; // KEYEVENT é 0, significa que o "botão" esta sendo pressionado

        int cuj = SendInput((short)sInputs.Length, sInputs, Marshal.SizeOf(new Input())); // vai retornar a quantidade de vezes que o input foi enviado com sucesso, se for igual ao tamanho do sInputs então todos os inputs funcionaram.
        Console.WriteLine(cuj);

        Thread.Sleep(1000); // o botão W vai ser segurado por 1 segundo antes de ser liberado, se for em um jogo, o personagem vai andar por 1 segundo.

        sInputs[0].type = INPUT_KEYBOARD;
        sInputs[0].ki.wVK = 0;
        sInputs[0].ki.wScan = MapVirtualKeyExA(VkKeyScanExA('w', GetKeyboardLayout(0)), MAPVK_VK_TO_VSC, GetKeyboardLayout(0));
        sInputs[0].ki.dwExtraInfo = GetMessageExtraInfo();
        sInputs[0].ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP; //Keyevent_keyup levanta o "botão"

        cuj = SendInput((short)sInputs.Length, sInputs, Marshal.SizeOf(new Input()));
    }
}
