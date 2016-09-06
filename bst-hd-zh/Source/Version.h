#ifndef __BLUESTACKS_VERSION_H__
#define __BLUESTACKS_VERSION_H__

#define XSTR(s)		VSTR(s)
#define VSTR(s)		#s

#define VERSION_STRING \
    XSTR(VERSION_MAJOR) "." XSTR(VERSION_MINOR) "." \
    XSTR(VERSION_PATCH) "." XSTR(VERSION_BUILD)

#define VERSION_PRODUCT		"BlueStacks"

#define VERSION_COMPANY		"BlueStack Systems"
#define VERSION_COPYRIGHT \
    "Copyright 2011 BlueStack Systems, Inc.  All Rights Reserved."

#endif /* !__BLUESTACKS_VERSION_H__ */
