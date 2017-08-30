var gulp = require('gulp');
var eslint = require('gulp-eslint');

gulp.task('lint', () => {
    return gulp.src([
        'Scripts/global.js',
        'Scripts/Administration/**/*.js',
        'Scripts/Contests/**/*.js',
        'Scripts/Helpers/**/*.js',
        '!node_modules/**'])
        .pipe(eslint())
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});
