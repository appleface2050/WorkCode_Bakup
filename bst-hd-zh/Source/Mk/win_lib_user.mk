!INCLUDE win_env_user.mk

all: $(LIB).lib

$(LIB).lib: $(OBJS)
	$(LD) /lib /nologo /out:$@ $(OBJS)

CLEAN = $(CLEAN) vc90.pdb

clean:
	DEL /F $(LIB).lib $(OBJS)
	DEL /F $(CLEAN)
