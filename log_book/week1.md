# Week 1

## Refactoring
We used 2to3, to determine what exactly was deprecated or changed between the versioning.

Among multible other things, we found that the functionality of the `werkzeug` package had been moved to `werkzeug.security` instead, so we updated this.


## `minitwit_tests.py`
For running the tests we ran into an issue that was that between python 2 and python 3 there has been a difference in how a string is represented. Before a string was represented as 'bytes', however it is in python 3 represented as a 'string' instead. This gives us a problem in the tests, as they in python 2 was written as:
```python
rv = self.register('user1', 'default')
assert 'The username is already taken' in rv.data
```
Here `rv.data` returns bytes, therefore in python 2, this comparison succeded, but not in python 3, because the comparison was then between a string and bytes. Therefore we fixed this by changing all the tests to:

```python
rv = self.register('user1', 'default')
assert 'The username is already taken' in rv.get_data(as_text=True)
```
Where `rv.get_data(as_text=True)`, then returns a string instead of bytes, and therefore this works. 

(Another fix could have been to change the strings to change the string to bytes, instead, however we went with this other way.)

## change in control.sh
We ran 
```cmd
shellcheck control.sh
```

Which added "quotation marks" around $ to prevent word splitting and globbing.


In addition to this we added which shell to target. We chose to target `#!/bin/sh`. We decided on this with a focus on portability, as the current control.sh does not have any specific bash syntax (bash-isms). We note that further changes to the control script might cause us to target a different shell.