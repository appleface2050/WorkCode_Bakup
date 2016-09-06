#include "Server.h"

/*******************************************************************++

Routine Description:
    The function to receive a request. This function calls the
    corresponding function to handle the response.

Arguments:
    hReqQueue - Handle to the request queue

Return Value:
    Success/Failure.

--*******************************************************************/
DWORD DoReceiveRequests(
    IN HANDLE hReqQueue
    )
{
    ULONG              result;
    HTTP_REQUEST_ID    requestId;
    DWORD              bytesRead;
    PHTTP_REQUEST      pRequest;
    PCHAR              pRequestBuffer;
    ULONG              RequestBufferLength;

    //
    // Allocate a 2 KB buffer. This size should work for most
    // requests. The buffer size can be increased if required. Space
    // is also required for an HTTP_REQUEST structure.
    //
    RequestBufferLength = sizeof(HTTP_REQUEST) + 2048;
    pRequestBuffer      = (PCHAR) ALLOC_MEM( RequestBufferLength );

    if (pRequestBuffer == NULL)
    {
        return ERROR_NOT_ENOUGH_MEMORY;
    }

    pRequest = (PHTTP_REQUEST)pRequestBuffer;

    //
    // Wait for a new request. This is indicated by a NULL
    // request ID.
    //

    HTTP_SET_NULL_ID( &requestId );

    for(;;)
    {
        RtlZeroMemory(pRequest, RequestBufferLength);

        result = HttpReceiveHttpRequest(
                    hReqQueue,          // Req Queue
                    requestId,          // Req ID
                    0,                  // Flags
                    pRequest,           // HTTP request buffer
                    RequestBufferLength,// req buffer length
                    &bytesRead,         // bytes received
                    NULL                // LPOVERLAPPED
                    );

	char* response;
	if(NO_ERROR == result)
        {
            //
            // Worked!
            //
            switch(pRequest->Verb)
            {
                case HttpVerbGET:
                    response = HandleHttpRequest(pRequest->CookedUrl.pFullUrl, NULL);
                    result = SendHttpResponse(
                                hReqQueue,
                                pRequest,
                                200,
                                "OK",
                                response
                                );
		    delete response;
                    break;

                case HttpVerbPOST:
                    result = SendHttpPostResponse(hReqQueue, pRequest);
                    break;

                default:
                    result = SendHttpResponse(
                                hReqQueue,
                                pRequest,
                                503,
                                "Not Implemented",
                                NULL
                                );
                    break;
            }

            if(result != NO_ERROR)
            {
                break;
            }

            //
            // Reset the Request ID to handle the next request.
            //
            HTTP_SET_NULL_ID( &requestId );
        }
        else if(result == ERROR_MORE_DATA)
        {
            //
            // The input buffer was too small to hold the request
            // headers. Increase the buffer size and call the
            // API again.
            //
            // When calling the API again, handle the request
            // that failed by passing a RequestID.
            //
            // This RequestID is read from the old buffer.
            //
            requestId = pRequest->RequestId;

            //
            // Free the old buffer and allocate a new buffer.
            //
            RequestBufferLength = bytesRead;
            FREE_MEM( pRequestBuffer );
            pRequestBuffer = (PCHAR) ALLOC_MEM( RequestBufferLength );

            if (pRequestBuffer == NULL)
            {
                result = ERROR_NOT_ENOUGH_MEMORY;
                break;
            }

            pRequest = (PHTTP_REQUEST)pRequestBuffer;

        }
        else if(ERROR_CONNECTION_INVALID == result &&
                !HTTP_IS_NULL_ID(&requestId))
        {
            // The TCP connection was corrupted by the peer when
            // attempting to handle a request with more buffer.
            // Continue to the next request.

            HTTP_SET_NULL_ID( &requestId );
        }
        else
        {
            break;
        }

    }

    if(pRequestBuffer)
    {
        FREE_MEM( pRequestBuffer );
    }

    return result;
}

/*******************************************************************++

Routine Description:
    The routine sends a HTTP response

Arguments:
    hReqQueue     - Handle to the request queue
    pRequest      - The parsed HTTP request
    StatusCode    - Response Status Code
    pReason       - Response reason phrase
    pEntityString - Response entity body

Return Value:
    Success/Failure.
--*******************************************************************/

DWORD SendHttpResponse(
    IN HANDLE        hReqQueue,
    IN PHTTP_REQUEST pRequest,
    IN USHORT        StatusCode,
    IN PSTR          pReason,
    IN PSTR          pEntityString
    )
{
    HTTP_RESPONSE   response;
    HTTP_DATA_CHUNK dataChunk;
    DWORD           result;
    DWORD           bytesSent;

    //
    // Initialize the HTTP response structure.
    //
    INITIALIZE_HTTP_RESPONSE(&response, StatusCode, pReason);

    //
    // Add a known header.
    //
    ADD_KNOWN_HEADER(response, HttpHeaderContentType, "text/html");

    if(pEntityString)
    {
        //
        // Add an entity chunk.
        //
        dataChunk.DataChunkType           = HttpDataChunkFromMemory;
        dataChunk.FromMemory.pBuffer      = pEntityString;
        dataChunk.FromMemory.BufferLength =
                                       (ULONG) strlen(pEntityString);

        response.EntityChunkCount         = 1;
        response.pEntityChunks            = &dataChunk;
    }

    //
    // Because the entity body is sent in one call, it is not
    // required to specify the Content-Length.
    //

    result = HttpSendHttpResponse(
                    hReqQueue,           // ReqQueueHandle
                    pRequest->RequestId, // Request ID
                    0,                   // Flags
                    &response,           // HTTP response
                    NULL,                // pReserved1
                    &bytesSent,          // bytes sent  (OPTIONAL)
                    NULL,                // pReserved2  (must be NULL)
                    0,                   // Reserved3   (must be 0)
                    NULL,                // LPOVERLAPPED(OPTIONAL)
                    NULL                 // pReserved4  (must be NULL)
                    );

    if(result != NO_ERROR)
    {
        wprintf(L"HttpSendHttpResponse failed with %lu \n", result);
    }

    return result;
}

#define MAX_ULONG_STR ((ULONG) sizeof("4294967295"))

/*******************************************************************++

Routine Description:
    The routine sends a HTTP response after reading the entity body.

Arguments:
    hReqQueue     - Handle to the request queue.
    pRequest      - The parsed HTTP request.

Return Value:
    Success/Failure.
--*******************************************************************/

DWORD SendHttpPostResponse(
    IN HANDLE        hReqQueue,
    IN PHTTP_REQUEST pRequest
    )
{
    HTTP_RESPONSE   response;
    DWORD           result;
    DWORD           bytesSent;
    PUCHAR          pEntityBuffer;
    ULONG           EntityBufferLength;
    ULONG           BytesRead;
    CHAR            szContentLength[MAX_ULONG_STR];
    HTTP_DATA_CHUNK dataChunk;
    ULONG           TotalBytesRead = 0;

    BytesRead  = 0;

    //
    // Allocate space for an entity buffer. Buffer can be increased
    // on demand.
    //
    EntityBufferLength = 2048;
    pEntityBuffer      = (PUCHAR) ALLOC_MEM( EntityBufferLength );

    if (pEntityBuffer == NULL)
    {
        result = ERROR_NOT_ENOUGH_MEMORY;
        wprintf(L"Insufficient resources \n");
        goto Done;
    }

    //
    // Initialize the HTTP response structure.
    //
    INITIALIZE_HTTP_RESPONSE(&response, 200, "OK");

    //
    // For POST, echo back the entity from the
    // client
    //
    // NOTE: If the HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY flag had been
    //       passed with HttpReceiveHttpRequest(), the entity would
    //       have been a part of HTTP_REQUEST (using the pEntityChunks
    //       field). Because that flag was not passed, there are no
    //       o entity bodies in HTTP_REQUEST.
    //

    char* postData = new char[MAX_PATH];
    int postDataChar = 0;
    char* responseStr;
    ULONG len;

    if(pRequest->Flags & HTTP_REQUEST_FLAG_MORE_ENTITY_BODY_EXISTS)
    {
        do
        {
            //
            // Read the entity chunk from the request.
            //
            BytesRead = 0;
            result = HttpReceiveRequestEntityBody(
                        hReqQueue,
                        pRequest->RequestId,
                        0,
                        pEntityBuffer,
                        EntityBufferLength,
                        &BytesRead,
                        NULL
                        );

            switch(result)
            {
                case NO_ERROR:

                    if(BytesRead != 0)
                    {
                        TotalBytesRead += BytesRead;

                        for (int j = 0; j < BytesRead; j++)
                            postData[postDataChar++] = pEntityBuffer[j];
                    }
                    break;

                case ERROR_HANDLE_EOF:

                    //
                    // The last request entity body has been read.
                    // Send back a response.
                    //
                    // To illustrate entity sends via
                    // HttpSendResponseEntityBody, the response will
                    // be sent over multiple calls. To do this,
                    // pass the HTTP_SEND_RESPONSE_FLAG_MORE_DATA
                    // flag.

                    if(BytesRead != 0)
                    {
                        TotalBytesRead += BytesRead;

                        for (int j = 0; j < BytesRead; j++)
                            postData[postDataChar++] = pEntityBuffer[j];
                    }

                    //
                    // Because the response is sent over multiple
                    // API calls, add a content-length.
                    //
                    // Alternatively, the response could have been
                    // sent using chunked transfer encoding, by
                    // passimg "Transfer-Encoding: Chunked".
                    //

                    // NOTE: Because the TotalBytesread in a ULONG
                    //       are accumulated, this will not work
                    //       for entity bodies larger than 4 GB.
                    //       For support of large entity bodies,
                    //       use a ULONGLONG.
                    //

                    responseStr = HandleHttpRequest(pRequest->CookedUrl.pFullUrl, postData);
                    len = (ULONG)(strlen(responseStr));

                    sprintf_s(szContentLength, MAX_ULONG_STR, "%lu", len);

                    ADD_KNOWN_HEADER(
                            response,
                            HttpHeaderContentLength,
                            szContentLength
                            );

                    result =
                        HttpSendHttpResponse(
                               hReqQueue,           // ReqQueueHandle
                               pRequest->RequestId, // Request ID
                               HTTP_SEND_RESPONSE_FLAG_MORE_DATA,
                               &response,       // HTTP response
                               NULL,            // pReserved1
                               &bytesSent,      // bytes sent-optional
                               NULL,            // pReserved2
                               0,               // Reserved3
                               NULL,            // LPOVERLAPPED
                               NULL             // pReserved4
                               );

                    if(result != NO_ERROR)
                    {
                        wprintf(
                           L"HttpSendHttpResponse failed with %lu \n",
                           result
                           );
                        goto Done;
                    }


                    //
                    // Add an entity chunk.
                    //
                    dataChunk.DataChunkType           = HttpDataChunkFromMemory;
                    dataChunk.FromMemory.pBuffer      = responseStr;
                    dataChunk.FromMemory.BufferLength = (ULONG) strlen(responseStr);

                    result = HttpSendResponseEntityBody(
                                hReqQueue,
                                pRequest->RequestId,
                                1,           // This is the last send.
                                1,           // Entity Chunk Count.
                                &dataChunk,
                                &len,
                                NULL,
                                0,
                                NULL,
                                NULL
                                );

                    if(result != NO_ERROR)
                    {
                       wprintf(
                          L"HttpSendResponseEntityBody failed %lu\n",
                          result
                          );
                    }

                    goto Done;

                    break;


                default:
                  wprintf(
                   L"HttpReceiveRequestEntityBody failed with %lu \n",
                   result);
                  goto Done;
            }

        } while(TRUE);
    }
    else
    {
        postData[postDataChar] = '\0';

        responseStr = HandleHttpRequest(pRequest->CookedUrl.pFullUrl, NULL);
        result = SendHttpResponse(
                    hReqQueue,
                    pRequest,
                    200,
                    "OK",
                    responseStr
                    );
    }

Done:

    delete responseStr;

    if(pEntityBuffer)
    {
        FREE_MEM(pEntityBuffer);
        printf("%s\n", postData);
    }

    return result;
}
