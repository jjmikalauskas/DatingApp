import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';

import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {

    constructor(private userService: UserService, private authService: AuthService,
         private router: Router, private alertify: AlertifyService) {
    }

    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        // const dToken = this.authService.decodedToken;
        const nameId = this.authService.decodedToken.nameid;
        // console.log(dToken, nameId);
        return this.userService.getUser(nameId).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving user in member-edit.resolver' + error);
                this.router.navigate(['/members']);
                return of(null);
             })
        );
    }
}
