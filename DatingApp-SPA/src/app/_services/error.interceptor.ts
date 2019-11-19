import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpEventType, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(
    req: import('@angular/common/http').HttpRequest<any>,
    next: import('@angular/common/http').HttpHandler
  ): import('rxjs').Observable<import('@angular/common/http').HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(httpError => {
        if (httpError.status === 401) {
          console.log('Exception 401');
          return throwError(httpError.statusText);
        }
        if (httpError instanceof HttpErrorResponse) {
            const applicationError = httpError.headers.get('Application-Error');
            console.log('applicationError', applicationError);
            if (applicationError) {
              return throwError(applicationError);
            }

            const serverError = httpError.error;
            let modelStateErrors = '';
            console.log('Server Error', serverError, ' and typeof=', typeof serverError);
            if (serverError && typeof serverError.errors === 'object') {
              // console.log('in serverError key array parsing');
              // window['se'] = serverError;
              for (const key in serverError) {
                if (serverError[key]) {
                  modelStateErrors += serverError[key] + '\n';
                }
              }
            }
            const retState = modelStateErrors || serverError || 'Server error';
            // if (modelStateErrors) {
            //   console.log('ModelStateErrors...', modelStateErrors);
            // } else if (serverError) {
            //   console.log('typeof= ', typeof serverError.errors);
            // }

            console.log('Return State=', retState);
            return throwError(retState);
        }
        console.log('Exception - Should not be here in the ErrorInterceptor...');
      })
    );
  }
}

export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true
};

