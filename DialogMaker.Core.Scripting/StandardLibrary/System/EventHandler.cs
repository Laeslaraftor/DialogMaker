namespace System;

public delegate void EventHandler(object? sender, EventArgs e);
public delegate void EventHandler<T>(object? sender, T e);