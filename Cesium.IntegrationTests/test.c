/*
 * SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
 *
 * SPDX-License-Identifier: MIT
 */

#include <stdlib.h>
#include <stdio.h>

int main(int argc, char *argv[])
{
    puts("test");
    int exitCode = abs(-42);
    exit(exitCode);
    char x = 'c';
    char y = '\t';
    char z = '\x32';
    char w = '\22';
    return 0;
}
