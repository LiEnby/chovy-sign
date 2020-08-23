#ifndef UTILS_H
#define UTILS_H
#include <stdio.h>
#define MAX(a, b) (((a) > (b)) ? (a) : (b))
#define MIN(a, b) (((a) < (b)) ? (a) : (b))
#define NELEMS(a) (sizeof(a) / sizeof(a[0]))

void hex_dump(const char *str, void *addr, int size);


FILE *open_file(char *name, int *size);
unsigned __int8 *load_file(char *name, int *size);
int write_file(char *file, void *buf, int size);
int isEmpty(unsigned char* buf, int buf_size);
int walk_dir(char *dname, void *func_ptr, int verbose);
void mkdir_p(char *dname);

#endif

