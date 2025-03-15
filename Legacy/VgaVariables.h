//Gore Engine
//Version 0.02 (BUILD 65)
//See History.h for history

#define KEY_LEFT  75
#define KEY_RIGHT 77
#define KEY_ESC   27
#define KEY_UP    72
#define KEY_DOWN  80

#define VIDEO_INT           0x10
#define SET_MODE            0x00
#define VGA_256_COLOR_MODE  0x13
#define TEXT_MODE           0x03

#define MISC_OUTPUT         0x03c2
#define SC_INDEX            0x03c4
#define SC_DATA             0x03c5
#define PALETTE_INDEX       0x03c8
#define PALETTE_DATA        0x03c9
#define CRTC_INDEX          0x03d4

#define MAP_MASK            0x02
#define MEMORY_MODE         0x04

#define H_TOTAL             0x00
#define H_DISPLAY_END       0x01
#define H_BLANK_START       0x02
#define H_BLANK_END         0x03
#define H_RETRACE_START     0x04
#define H_RETRACE_END       0x05
#define V_TOTAL             0x06
#define OVERFLOW            0x07
#define MAX_SCAN_LINE       0x09
#define V_RETRACE_START     0x10
#define V_RETRACE_END       0x11
#define V_DISPLAY_END       0x12
#define OFFSET              0x13
#define UNDERLINE_LOCATION  0x14
#define V_BLANK_START       0x15
#define V_BLANK_END         0x16
#define MODE_CONTROL        0x17

#define NUM_COLORS          256

#define sgn(x)\
((x<0)?-1:((x>0)?1:0))

char pal[256*3];
int x,y,choice=1;

int x_size[1]={320};
int y_size[1]={200};

#define word_out(port,register,value) \
outpw(port,(((word)value<<8) + register))

int keybuf;

typedef unsigned char  byte;
typedef unsigned short word;
typedef unsigned long  dword;
word *my_clock=(word *)0x0000046C;

byte *VGA=(byte *)0xA0000000L;
word screen_width, screen_height;