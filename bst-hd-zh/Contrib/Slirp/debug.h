/*
 * Copyright (c) 1995 Danny Gasparovski.
 *
 * Please read the file COPYRIGHT for the
 * terms and conditions of the copyright.
 */

extern int dostats;
extern int slirp_debug;

#define DBG_CALL 0x1
#define DBG_MISC 0x2
#define DBG_ERROR 0x4
#define DEBUG_DEFAULT DBG_CALL|DBG_MISC|DBG_ERROR

#ifdef DEBUG

extern void LoggerPrint(const char *fmt, ...);

#define DEBUG_CALL(x)	if (slirp_debug & DBG_CALL)	LoggerPrint("%s()", x)
#define DEBUG_ARG(x, y)	if (slirp_debug & DBG_CALL)	LoggerPrint(" " x, y)
#define DEBUG_ARGS(x)	if (slirp_debug & DBG_CALL)	LoggerPrint x
#define DEBUG_MISC(x)	if (slirp_debug & DBG_MISC)	LoggerPrint x
#define DEBUG_ERROR(x)	if (slirp_debug & DBG_ERROR)	LoggerPrint x

#else

#define DEBUG_CALL(x)	((void)0)
#define DEBUG_ARG(x, y)	((void)0)
#define DEBUG_ARGS(x)	((void)0)
#define DEBUG_MISC(x)	((void)0)
#define DEBUG_ERROR(x)	((void)0)

#endif

void debug_init _P((int));
//void ttystats _P((struct ttys *));
void allttystats _P((void));
void ipstats _P((void));
void vjstats _P((void));
void tcpstats _P((void));
void udpstats _P((void));
void icmpstats _P((void));
void mbufstats _P((void));
void sockstats _P((void));
void slirp_exit _P((int));

