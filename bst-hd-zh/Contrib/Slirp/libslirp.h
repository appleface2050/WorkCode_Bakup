#ifndef _LIBSLIRP_H
#define _LIBSLIRP_H

#ifdef _WIN32
#define FD_SETSIZE	512	/* XXX must match slirp.h */
#include <winsock2.h>
int inet_aton(const char *cp, struct in_addr *ia);
/* #include <stdint.h> */
#else
#include <sys/select.h>
#include <arpa/inet.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

void slirp_init(void);

void slirp_select_fill(int *pnfds,
                       fd_set *readfds, fd_set *writefds, fd_set *xfds);

void slirp_select_poll(fd_set *readfds, fd_set *writefds, fd_set *xfds);

void slirp_input(const unsigned char *pkt, int pkt_len);

/* you must provide the following functions: */
int slirp_can_output(void);
void slirp_output(const unsigned char *pkt, int pkt_len);

int slirp_redir(int is_udp, int host_port,
                struct in_addr guest_addr, int guest_port,
                int listen_on_wildcard);

extern const char *tftp_prefix;
extern char slirp_hostname[33];

void slirp_debug_dump(void);

#ifdef __cplusplus
}
#endif

#endif
