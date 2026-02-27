import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { ConnectionService } from '../shared/services/connection.service';

export const userResolver: ResolveFn<Object> = (route, state) => {
  const connectionService = inject(ConnectionService);

  return connectionService.getUser();
};
