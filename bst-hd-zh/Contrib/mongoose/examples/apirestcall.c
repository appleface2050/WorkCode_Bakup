#include "mongoose.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

static const char *s_no_cache_header =
  "Cache-Control: max-age=0, post-check=0, "
  "pre-check=0, no-store, no-cache, must-revalidate\r\n";

static void handle_restful_int_call(struct mg_connection *conn) {
  char n1[100], n2[100];

  // Get form variables
  mg_get_var(conn, "n1", n1, sizeof(n1));
  mg_get_var(conn, "n2", n2, sizeof(n2));

  mg_printf_data(conn, "{ \"n1\": %d,\"n2\": %d }", atoi(n1), atoi(n2));
}

static void handle_restful_string_call(struct mg_connection *conn) {
  char n1[100], n2[10];

  // Get form variables
  mg_get_var(conn, "n1", n1, sizeof(n1));
  mg_get_var(conn, "n2", n2, sizeof(n2));

  mg_printf_data(conn, "{ \"result\": %s %s}", n1 ,n2);
}

static int ev_handler(struct mg_connection *conn, enum mg_event ev) {
  switch (ev) {
    case MG_AUTH: return MG_TRUE;
    case MG_REQUEST:
      if (!strcmp(conn->uri, "/print/int")) {
        handle_restful_int_call(conn);
        return MG_TRUE;
      }
	  else  if (!strcmp(conn->uri, "/print/string")) {
        handle_restful_string_call(conn);
        return MG_TRUE;
      }
      mg_send_file(conn, "index.html", s_no_cache_header);
      return MG_MORE;
    default: return MG_FALSE;
  }
}

int main(void) {
  struct mg_server *server = mg_create_server(NULL, ev_handler);
  mg_set_option(server, "document_root", "/Users/ashish/Downloads/testmangoose");      // Serve current directory
  mg_set_option(server, "listening_port", "9080");  // Open port 8080

  for (;;) {
    mg_poll_server(server, 1000);   // Infinite loop, Ctrl-C to stop
  }
  mg_destroy_server(&server);

  return 0;
}
