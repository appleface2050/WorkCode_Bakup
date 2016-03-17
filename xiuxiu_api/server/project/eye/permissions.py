from rest_framework import permissions
from rest_framework.authtoken.models import Token


class IsOwnerOrReadOnly(permissions.BasePermission):
    """
    Custom permission to only allow owners of an object to edit it.
    """

    def has_object_permission(self, request, view, obj):
        # Read permissions are allowed to any request,
        # so we'll always allow GET, HEAD or OPTIONS requests.
        if request.method in permissions.SAFE_METHODS:
            return True

        # Write permissions are only allowed to the owner of the snippet.
        return obj.owner == request.user


class IsAuthenticatedOrReadOnly(permissions.BasePermission):
    # def has_object_permission(self, request, view, obj):
    def has_permission(self, request, view):
        # Read permissions are allowed to any request,
        # so we'll always allow GET, HEAD or OPTIONS requests.
        if request.method in permissions.SAFE_METHODS:
            return True

        user = request.user
        if user.is_authenticated():
            return True

        key = request.data.get("key", None)
        user_id = int(request.data.get("user_id", 0))
        if not (key and user_id):
            return False
        else:
            user_token = Token.objects.filter(key=key)
            if user_token.count() == 1:
                if user_id == user_token[0].user_id:
                    return True
                else:
                    return False
            else:
                return False
