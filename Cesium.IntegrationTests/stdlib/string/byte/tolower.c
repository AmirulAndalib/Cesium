/*
 * SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
 *
 * SPDX-License-Identifier: MIT
 */

#include <ctype.h>
#include <limits.h>
#include <stdio.h>

int main(void)
{
    /* In the default locale: */
    for (unsigned char l = 0; l < UCHAR_MAX; l++) {
        unsigned char u = tolower(l);
        if (u != l) printf("%c%c ", l, u);
    }
    printf("\n\n");

    return 42;
}
