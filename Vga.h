//Gore Engine
//Version 0.02 (BUILD 65)
//See History.h for history

#include "Variables.h"

void set_mode(byte mode)
{
  union REGS regs;

  regs.h.ah = SET_MODE;
  regs.h.al = mode;
  int86(VIDEO_INT, &regs, &regs);
}

void set_unchained_mode()
{
  word i;
  dword *ptr=(dword *)VGA;

  set_mode(VGA_256_COLOR_MODE);

  word_out(SC_INDEX, MEMORY_MODE,0x06);

  word_out(SC_INDEX, MAP_MASK, 0xff);

  for(i=0;i<0x4000;i++)
    *ptr++ = 0;

  word_out(CRTC_INDEX, UNDERLINE_LOCATION, 0x00);

  word_out(CRTC_INDEX, MODE_CONTROL, 0xe3);

  screen_width=320;
  screen_height=200;

}

void wait(int ticks)
{
  word start;

  start=*my_clock;

  while (*my_clock-start<ticks)
  {
    *my_clock=*my_clock;
  }
}