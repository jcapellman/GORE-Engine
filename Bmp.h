//Gore Engine
//Version 0.02 (BUILD 65)
//See History.h for history

char pal[256*3];

void fskip(FILE *fp, int num_bytes)
{
   int i;
   for (i=0; i<num_bytes; i++)
      fgetc(fp);
}

void setpal(unsigned char*pal)
{
     int i;
     outp(0x3c8,0);
     for(i=0;i<256*3;i++)
          outp(0x3c9,pal[i]);
}

void draw_bitmap(PLANAR_BITMAP *bmp,int x,int y)
{
  int j,plane;
  word screen_offset;
  word bitmap_offset;

  for(plane=0; plane<4; plane++)
  {
    outp(SC_INDEX, MAP_MASK);          /* select plane */
    outp(SC_DATA,  1 << ((plane+x)&3) );
    bitmap_offset=0;
    screen_offset = ((dword)y*screen_width+x+plane) >> 2;
    for(j=0; j<bmp->height; j++)
    {
      memcpy(&VGA[screen_offset], &bmp->data[plane][bitmap_offset], (bmp->width >> 2));

      bitmap_offset+=bmp->width>>2;
      screen_offset+=screen_width>>2;
    }
  }
}

void load_bmp(char *file,PLANAR_BITMAP *b)
{
  FILE *fp;
  long index, size;
  word num_colors;
  int x, plane;

  if ((fp = fopen(file,"rb")) == NULL)
  {
    printf("Error opening file %s.\n",file);
    exit(1);
  }

  if (fgetc(fp)!='B' || fgetc(fp)!='M')
  {
    fclose(fp);
    printf("%s is not a bitmap file.\n",file);
    exit(1);
  }

  fskip(fp,16);
  fread(&b->width, sizeof(word), 1, fp);
  fskip(fp,2);
  fread(&b->height,sizeof(word), 1, fp);
  fskip(fp,22);
  fread(&num_colors,sizeof(word), 1, fp);
  fskip(fp,6);

  if (num_colors==0) num_colors=256;

  size=b->width*b->height;
  for(plane=0;plane<4;plane++)
  {
    if ((b->data[plane] = (byte *) malloc((word)(size>>2))) == NULL)
    {
      fclose(fp);
      printf("Error allocating memory for file %s.\n",file);
      exit(1);
    }
  }

  for(index=0;index<num_colors;index++)
  {
    b->palette[(int)(index*3+2)] = fgetc(fp) >> 2;
    b->palette[(int)(index*3+1)] = fgetc(fp) >> 2;
    b->palette[(int)(index*3+0)] = fgetc(fp) >> 2;
    x=fgetc(fp);
  }

  for(index = (b->height-1)*b->width; index >= 0;index-=b->width)
    for(x = 0; x < b->width; x++)
      b->data[x&3][(int)((index+x)>>2)]=(byte)fgetc(fp);

  fclose(fp);
}

void set_palette(byte *palette)
{
  int i;

  outp(PALETTE_INDEX,0);              /* tell the VGA that palette data
                                         is coming. */
  for(i=0;i<256*3;i++)
    outp(PALETTE_DATA,palette[i]);    /* write the data */
}

void plot_pixel(int x,int y,byte color)
{
  dword offset;

  outp(SC_INDEX, MAP_MASK);          /* select plane */
  outp(SC_DATA,  1 << (x&3) );

  offset=((dword)y*screen_width+x) >> 2;

  VGA[(word)offset]=color;
}
