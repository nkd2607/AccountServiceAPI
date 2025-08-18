namespace AccountService.Exceptions;

public class ConcurrencyException() : Exception("«апись была изменена другим пользователем");